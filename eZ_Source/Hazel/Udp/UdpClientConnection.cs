﻿using System;
using System.Net;
using System.Net.Sockets;


namespace Hazel.Udp
{
    /// <summary>
    ///     Represents a client's connection to a server that uses the UDP protocol.
    /// </summary>
    /// <inheritdoc/>
    public sealed class UdpClientConnection : UdpConnection
    {
        /// <summary>
        ///     The socket we're connected via.
        /// </summary>
        Socket socket;

        /// <summary>
        ///     The lock for the socket.
        /// </summary>
        Object socketLock = new Object();

        /// <summary>
        ///     The buffer to store incomming data in.
        /// </summary>
        byte[] dataBuffer = new byte[ushort.MaxValue];

        /// <summary>
        ///     Creates a new UdpClientConnection.
        /// </summary>
        /// <param name="remoteEndPoint">A <see cref="NetworkEndPoint"/> to connect to.</param>
        public UdpClientConnection(NetworkEndPoint remoteEndPoint) : base()
        {
            lock (socketLock)
            {
                this.EndPoint = remoteEndPoint;
                this.RemoteEndPoint = remoteEndPoint.EndPoint;
                this.IPMode = remoteEndPoint.IPMode;

                if (remoteEndPoint.IPMode == IPMode.IPv4)
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                else
                {
                    if (!Socket.OSSupportsIPv6)
                        throw new HazelException("IPV6 not supported!");

                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    socket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);    //TODO these lines shouldn't be needed anymore
                }
            }
        }

        /// <inheritdoc />
        protected override void WriteBytesToConnection(byte[] bytes)
        {
            //Pack
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(bytes, 0, bytes.Length);
            args.RemoteEndPoint = RemoteEndPoint;

            lock (socketLock)
            {
                if (State != ConnectionState.Connected && State != ConnectionState.Connecting)
                    throw new InvalidOperationException("Could not send data as this Connection is not connected and is not connecting. Did you disconnect?");

                try
                {
                    socket.SendToAsync(args);
                }
                catch (ObjectDisposedException)
                {
                    //User probably called Disconnect in between this method starting and here so report the issue
                    throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
                }
                catch (SocketException e)
                {
                    HazelException he = new HazelException("Could not send data as a SocketException occured.", e);
                    HandleDisconnect(he);
                    throw he;
                }
            }
        }

        /// <inheritdoc />
        public override void Connect()
        {
            lock(socketLock)
            {
                if (State != ConnectionState.NotConnected)
                    throw new InvalidOperationException("Cannot connect as the Connection is already connected.");

                State = ConnectionState.Connecting;

                //Begin listening
                try
                {
                    if (IPMode == IPMode.IPv4)
                        socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                    else
                        socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
                }
                catch (SocketException e)
                {
                    throw new HazelException("A socket exception occured while binding to the port.", e);
                }

                try
                {
                    StartListeningForData();
                }
                catch (ObjectDisposedException)
                {
                    //If the socket's been disposed then we can just end there but make sure we're in NotConnected state.
                    //If we end up here I'm really lost...
                    State = ConnectionState.NotConnected;
                    return;
                }
                catch (SocketException e)
                {
                    throw new HazelException("A Socket exception occured while initiating a receive operation.", e);
                }
            }

            //Write bytes to the server to tell it hi (and to punch a hole in our NAT, if present)
            //When acknowledged set the state to connected
            SendHello(() => { lock (socketLock) State = ConnectionState.Connected; });

            //Wait till hello packet is acknowledged and the state is set to Connected
            WaitOnConnect();
        }

        /// <summary>
        ///     Instructs the listener to begin listening.
        /// </summary>
        void StartListeningForData()
        {
            lock (socketLock)
                socket.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ReadCallback, dataBuffer);
        }

        /// <summary>
        ///     Called when data has been received by the socket.
        /// </summary>
        /// <param name="result">The asyncronous operation's result.</param>
        void ReadCallback(IAsyncResult result)
        {
            int bytesReceived;

            //End the receive operation
            try
            {
                lock (socketLock)
                    bytesReceived = socket.EndReceive(result);
            }
            catch (ObjectDisposedException)
            {
                //If the socket's been disposed then we can just end there.
                return;
            }
            catch (SocketException e)
            {
                HandleDisconnect(new HazelException("A socket exception occured while reading data.", e));
                return;
            }

            //Exit if no bytes read, we've failed.
            if (bytesReceived == 0)
            {
                HandleDisconnect();
                return;
            }

            //Decode the data received
            byte[] buffer = HandleReceive(dataBuffer, bytesReceived);
            SendOption sendOption = (SendOption)dataBuffer[0];

            //TODO may get better performance with Handle receive after and block copy call added

            //Begin receiving again
            try
            {
                StartListeningForData();
            }
            catch (SocketException e)
            {
                HandleDisconnect(new HazelException("A Socket exception occured while initiating a receive operation.", e));
            }
            catch (ObjectDisposedException)
            {
                //If the socket's been disposed then we can just end there.
                return;
            }
            
            if (buffer != null)
                InvokeDataReceived(buffer, sendOption);
        }

        /// <inheritdoc />
        protected override void HandleDisconnect(HazelException e = null)
        {
            bool invoke = false;

            lock (socketLock)
            {
                //Only invoke the disconnected event if we're not already disconnecting
                if (State == ConnectionState.Connected)
                {
                    State = ConnectionState.Disconnecting;
                    invoke = true;
                }
            }

            //Invoke event outide lock if need be
            if (invoke)
            {
                InvokeDisconnected(e);

                Dispose();
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Send disconnect message if we're not already disconnecting
                if (State == ConnectionState.Connected)
                    SendDisconnect();

                //Dispose of the socket
                lock (socketLock)
                {
                    State = ConnectionState.NotConnected;

                    socket.Close();
                }
            }

            base.Dispose(disposing);
        }
    }
}