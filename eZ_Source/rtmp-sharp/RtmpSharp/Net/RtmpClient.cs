﻿// Decompiled with JetBrains decompiler
// Type: RtmpSharp.Net.RtmpClient
// Assembly: rtmp-sharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8588136F-A4B9-4004-9712-4EA13AA4AF9D
// Assembly location: C:\Users\Hesa\Desktop\eZ_Source\bin\Debug\rtmp-sharp.dll

using Complete;
using Complete.Threading;
using RtmpSharp.IO;
using RtmpSharp.Messaging;
using RtmpSharp.Messaging.Events;
using RtmpSharp.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtmpSharp.Net
{
    public class RtmpClient
    {
        private readonly RemoteCertificateValidationCallback _certificateValidator = (RemoteCertificateValidationCallback)((sender, certificate, chain, errors) => true);
        public bool NoDelay = true;
        private readonly TaskCallbackManager<int, AcknowledgeMessageExt> _callbackManager;
        private readonly ObjectEncoding _objectEncoding;
        private readonly Uri _uri;
        private readonly SerializationContext _serializationContext;
        public string ClientId;
        public bool ExclusiveAddressUse;
        private int _invokeId;
        public IPEndPoint LocalEndPoint;
        private RtmpPacketReader _reader;
        private Thread _readerThread;
        public int ReceiveTimeout;
        private string _reconnectData;
        public int SendTimeout;
        private RtmpPacketWriter _writer;
        private Thread _writerThread;

        public bool IsDisconnected { get; set; }

        public event EventHandler Disconnected;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public event EventHandler<Exception> CallbackException;

        public RtmpClient(Uri uri, SerializationContext serializationContext)
        {
            if (uri == (Uri)null)
                throw new ArgumentNullException("uri");
            if (serializationContext == null)
                throw new ArgumentNullException("serializationContext");
            string lowerInvariant = uri.Scheme.ToLowerInvariant();
            if (lowerInvariant != "rtmp" && lowerInvariant != "rtmps")
                throw new ArgumentException("Only rtmp:// and rtmps:// connections are supported.");
            this._uri = uri;
            this._serializationContext = serializationContext;
            this._callbackManager = new TaskCallbackManager<int, AcknowledgeMessageExt>();
        }

        public RtmpClient(Uri uri, SerializationContext serializationContext, ObjectEncoding objectEncoding)
          : this(uri, serializationContext)
        {
            this._objectEncoding = objectEncoding;
        }

        public RtmpClient(Uri uri, ObjectEncoding objectEncoding, SerializationContext serializationContext, RemoteCertificateValidationCallback certificateValidator)
          : this(uri, serializationContext, objectEncoding)
        {
            if (certificateValidator == null)
                throw new ArgumentNullException("certificateValidator");
            this._certificateValidator = certificateValidator;
        }

        private Task<AcknowledgeMessageExt> QueueCommandAsTask(Command command, int streamId, int messageStreamId, bool requireConnected = true)
        {
            if (requireConnected && this.IsDisconnected)
                return RtmpClient.CreateExceptedTask((Exception)new ClientDisconnectedException("disconnected"));
            Task<AcknowledgeMessageExt> task = this._callbackManager.Create(command.InvokeId);
            this._writer.Queue((RtmpEvent)command, streamId, messageStreamId);
            return task;
        }

        public async Task<AsObject> ConnectAsync()
        {
            TcpClient client = this.CreateTcpClient();
            client.NoDelay = this.NoDelay;
            client.ReceiveTimeout = this.ReceiveTimeout;
            client.SendTimeout = this.SendTimeout;
            client.ExclusiveAddressUse = this.ExclusiveAddressUse;
            await client.ConnectAsync(this._uri.Host, this._uri.Port);
            Stream stream = await this.GetRtmpStreamAsync(client);
            Random random = new Random();
            byte[] numArray = new byte[1528];
            byte[] buffer = numArray;
            random.NextBytes(buffer);
            RtmpHandshake c01 = new RtmpHandshake() { Version = 3, Time = (uint)Environment.TickCount, Time2 = 0, Random = numArray };
            await RtmpHandshake.WriteAsync(stream, c01, true);
            RtmpHandshake h = (await RtmpHandshake.ReadAsync(stream, true)).Clone();
            h.Time2 = (uint)Environment.TickCount;
            await RtmpHandshake.WriteAsync(stream, h, false);
            RtmpHandshake rtmpHandshake = await RtmpHandshake.ReadAsync(stream, false);
            if (!((IEnumerable<byte>)c01.Random).SequenceEqual<byte>((IEnumerable<byte>)rtmpHandshake.Random) || (int)c01.Time != (int)rtmpHandshake.Time)
                throw new ProtocolViolationException();
            this.EstablishThreads(stream);
            AsObject asObject = await this.ConnectInvokeAsync((string)null, (string)null, this._uri.ToString());
            string key1 = "clientId";
            object obj;

            if (asObject.TryGetValue(key1, out obj))
                this.ClientId = obj as string;
            string key2 = "id";
            if (asObject.TryGetValue(key2, out obj))
                this.ClientId = this.ClientId ?? obj as string;
            return asObject;
        }

        public async Task<AcknowledgeMessageExt> ConnectAckAsync()
        {
            AcknowledgeMessageExt acknowledgeMessageExt1 = new AcknowledgeMessageExt();
            AcknowledgeMessageExt acknowledgeMessageExt2 = acknowledgeMessageExt1;
            AsObject asObject = await this.ConnectAsync();
            acknowledgeMessageExt2.Body = (object)asObject;
            return acknowledgeMessageExt1;
        }

        public async Task<AsObject> ReconnectAsync()
        {
            TcpClient client = this.CreateTcpClient();
            client.NoDelay = this.NoDelay;
            client.ReceiveTimeout = this.ReceiveTimeout;
            client.SendTimeout = this.SendTimeout;
            client.ExclusiveAddressUse = this.ExclusiveAddressUse;
            await client.ConnectAsync(this._uri.Host, this._uri.Port);
            Stream stream = await this.GetRtmpStreamAsync(client);
            Random random = new Random();
            byte[] numArray = new byte[1528];
            byte[] buffer = numArray;
            random.NextBytes(buffer);
            RtmpHandshake c01 = new RtmpHandshake() { Version = 3, Time = (uint)Environment.TickCount, Time2 = 0, Random = numArray };
            await RtmpHandshake.WriteAsync(stream, c01, true);
            RtmpHandshake h = (await RtmpHandshake.ReadAsync(stream, true)).Clone();
            h.Time2 = (uint)Environment.TickCount;
            await RtmpHandshake.WriteAsync(stream, h, false);
            RtmpHandshake rtmpHandshake = await RtmpHandshake.ReadAsync(stream, false);
            if (!((IEnumerable<byte>)c01.Random).SequenceEqual<byte>((IEnumerable<byte>)rtmpHandshake.Random) || (int)c01.Time != (int)rtmpHandshake.Time)
                throw new ProtocolViolationException();
            this.EstablishThreads(stream);
            AsObject asObject = await this.ReconnectInvokeAsync((string)null, (string)null, this._uri.ToString());
            this.IsDisconnected = false;
            return asObject;
        }

        public async Task<AcknowledgeMessageExt> ReconnectAckAsync()
        {
            AcknowledgeMessageExt acknowledgeMessageExt1 = new AcknowledgeMessageExt();
            AcknowledgeMessageExt acknowledgeMessageExt2 = acknowledgeMessageExt1;
            AsObject asObject = await this.ReconnectAsync();
            acknowledgeMessageExt2.Body = (object)asObject;
            return acknowledgeMessageExt1;
        }

        public void EstablishThreads(Stream stream)
        {
            this._writer = new RtmpPacketWriter(new AmfWriter(stream, this._serializationContext), ObjectEncoding.Amf3);
            this._reader = new RtmpPacketReader(new AmfReader(stream, this._serializationContext));
            this._reader.EventReceived += new EventHandler<EventReceivedEventArgs>(this.EventReceivedCallback);
            this._reader.Disconnected += new EventHandler<ExceptionalEventArgs>(this.OnPacketProcessorDisconnected);
            this._writer.Disconnected += new EventHandler<ExceptionalEventArgs>(this.OnPacketProcessorDisconnected);
            this._writerThread = new Thread(new ThreadStart(this._reader.ReadLoop))
            {
                IsBackground = true
            };
            this._readerThread = new Thread(new ThreadStart(this._writer.WriteLoop))
            {
                IsBackground = true
            };
            this._writerThread.Start();
            this._readerThread.Start();
        }

        public void Close()
        {
            this.OnDisconnected(new ExceptionalEventArgs("closed"));
        }

        private TcpClient CreateTcpClient()
        {
            if (this.LocalEndPoint == null)
                return new TcpClient();
            return new TcpClient(this.LocalEndPoint);
        }

        protected virtual async Task<Stream> GetRtmpStreamAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            string scheme = this._uri.Scheme;
            if (scheme == "rtmp")
                return (Stream)stream;
            if (!(scheme == "rtmps"))
                throw new ArgumentException("The specified scheme is not supported.");
            SslStream ssl = new SslStream((Stream)stream, false, this._certificateValidator);
            await ssl.AuthenticateAsClientAsync(this._uri.Host);
            return (Stream)ssl;
        }

        private void OnPacketProcessorDisconnected(object sender, ExceptionalEventArgs e)
        {
            this.OnDisconnected(e);
        }

        private void OnDisconnected(ExceptionalEventArgs e)
        {
            if (this.IsDisconnected)
                return;
            this.IsDisconnected = true;
            if (this._writer != null)
                this._writer.Continue = false;
            if (this._reader != null)
                this._reader.Continue = false;
            this.WrapCallback((Action)(() => this._callbackManager.SetExceptionForAll((Exception)new ClientDisconnectedException(e.Description, e.Exception))));
            this._invokeId = 0;
            this.WrapCallback((Action)(() =>
           {
               // ISSUE: reference to a compiler-generated field
               EventHandler disconnected = this.Disconnected;
               if (disconnected == null)
                   return;
               ExceptionalEventArgs exceptionalEventArgs = e;
               disconnected((object)this, (EventArgs)exceptionalEventArgs);
           }));
        }

        private void EventReceivedCallback(object sender, EventReceivedEventArgs e)
        {
            try
            {
                switch (e.Event.MessageType)
                {
                    case MessageType.UserControlMessage:
                    UserControlMessage userControlMessage = (UserControlMessage)e.Event;
                    if (userControlMessage.EventType != UserControlMessageType.PingRequest)
                        break;
                    this.WriteProtocolControlMessage((RtmpEvent)new UserControlMessage(UserControlMessageType.PingResponse, userControlMessage.Values));
                    break;

                    case MessageType.CommandAmf3:
                    case MessageType.DataAmf0:
                    case MessageType.CommandAmf0:
                    Command command = (Command)e.Event;
                    Method methodCall = command.MethodCall;
                    object obj1 = methodCall.Parameters.Length == 1 ? methodCall.Parameters[0] : (object)methodCall.Parameters;
                    if (methodCall.Name == "_result" && !(obj1 is AcknowledgeMessageExt))
                    {
                        AcknowledgeMessageExt acknowledgeMessageExt = new AcknowledgeMessageExt();
                        object obj2 = obj1;
                        acknowledgeMessageExt.Body = obj2;
                        AcknowledgeMessageExt result = acknowledgeMessageExt;
                        this._callbackManager.SetResult(command.InvokeId, result);
                        break;
                    }
                    if (methodCall.Name == "_result")
                    {
                        AcknowledgeMessageExt result = (AcknowledgeMessageExt)obj1;
                        this._callbackManager.SetResult(command.InvokeId, result);
                        break;
                    }
                    if (methodCall.Name == "_error")
                    {
                        ErrorMessage errorMessage = (ErrorMessage)obj1;
                        this._callbackManager.SetException(command.InvokeId, errorMessage != null ? (Exception)new InvocationException(errorMessage) : (Exception)new InvocationException());
                        break;
                    }
                    if (methodCall.Name == "receive")
                    {
                        AsyncMessageExt message = (AsyncMessageExt)obj1;
                        if (message == null)
                            break;
                        object obj2;
                        message.Headers.TryGetValue("DSSubtopic", out obj2);
                        string dsSubtopic = obj2 as string;
                        string clientId = message.ClientId;
                        this.WrapCallback((Action)(() =>
                       {
                           // ISSUE: reference to a compiler-generated field
                           EventHandler<MessageReceivedEventArgs> messageReceived = this.MessageReceived;
                           if (messageReceived == null)
                               return;
                           MessageReceivedEventArgs e1 = new MessageReceivedEventArgs(clientId, dsSubtopic, message);
                           messageReceived((object)this, e1);
                       }));
                        break;
                    }
                    int num = methodCall.Name == "onstatus" ? 1 : 0;
                    break;
                }
            }
            catch (ClientDisconnectedException ex)
            {
                this.Close();
            }
        }

        public Task<T> InvokeAsync<T>(string method, object argument)
        {
            return this.InvokeAsync<T>(method, new object[1] { argument });
        }

        public async Task<T> InvokeAsync<T>(string method, object[] arguments)
        {
            InvokeAmf0 invokeAmf0 = new InvokeAmf0();
            Method method1 = new Method(method, arguments, true, CallStatus.Request);
            invokeAmf0.MethodCall = method1;
            int nextInvokeId = this.GetNextInvokeId();
            invokeAmf0.InvokeId = nextInvokeId;
            return (T)MiniTypeConverter.ConvertTo((await this.QueueCommandAsTask((Command)invokeAmf0, 3, 0, true)).Body, typeof(T));
        }

        public Task<T> InvokeAsync<T>(string endpoint, string destination, string method, object argument)
        {
            string endpoint1 = endpoint;
            string destination1 = destination;
            string method1 = method;
            object[] arguments;
            if (!(argument is object[]))
                arguments = new object[1] { argument };
            else
                arguments = (object[])argument;
            return this.InvokeAsync<T>(endpoint1, destination1, method1, arguments);
        }

        internal async Task<T> InvokeAsync<T>(RemotingMessage message)
        {
            InvokeAmf3 invokeAmf3 = new InvokeAmf3();
            invokeAmf3.InvokeId = this.GetNextInvokeId();
            invokeAmf3.MethodCall = new Method((string)null, new object[1]
            {
        (object) message
            }, 1 != 0, CallStatus.Request);
            return (T)MiniTypeConverter.ConvertTo((await this.QueueCommandAsTask((Command)invokeAmf3, 3, 0, true)).Body, typeof(T));
        }

        public async Task<T> InvokeAsync<T>(string endpoint, string destination, string method, object[] arguments)
        {
            if (this._objectEncoding != ObjectEncoding.Amf3)
                throw new NotSupportedException("Flex RPC requires AMF3 encoding.");
            RemotingMessage remotingMessage1 = new RemotingMessage();
            string clientId = this.ClientId;
            remotingMessage1.ClientId = clientId;
            string str1 = destination;
            remotingMessage1.Destination = str1;
            string str2 = method;
            remotingMessage1.Operation = str2;
            object[] objArray = arguments;
            remotingMessage1.Body = (object)objArray;
            remotingMessage1.Headers = new AsObject()
            {
                {
                  "DSEndpoint",
                  (object) endpoint
                },
                {
                  "DSId",
                  (object) (this.ClientId ?? "nil")
                },
                {
                  "DSRequestTimeout",
                  (object) 60
                }
            };
            RemotingMessage remotingMessage2 = remotingMessage1;
            InvokeAmf3 invokeAmf3 = new InvokeAmf3();
            invokeAmf3.InvokeId = this.GetNextInvokeId();
            invokeAmf3.MethodCall = new Method((string)null, new object[1]
            {
                remotingMessage2
            }, 1 != 0, CallStatus.Request);
            return (T)MiniTypeConverter.ConvertTo((await this.QueueCommandAsTask(invokeAmf3, 3, 0, true)).Body, typeof(T));
        }

        private async Task<AsObject> ConnectInvokeAsync(string pageUrl, string swfUrl, string tcUrl)
        {
            InvokeAmf0 invokeAmf0_1 = new InvokeAmf0();
            InvokeAmf0 invokeAmf0_2 = invokeAmf0_1;
            string methodName = "connect";
            object[] parameters = new object[4] { (object)false, (object)"nil", (object)"", null };
            int index = 3;
            CommandMessage commandMessage = new CommandMessage();
            commandMessage.Operation = CommandOperation.ClientPing;
            string str1 = "";
            commandMessage.CorrelationId = str1;
            string str2 = Uuid.NewUuid();
            commandMessage.MessageId = str2;
            string str3 = "";
            commandMessage.Destination = str3;
            commandMessage.Headers = new AsObject()
      {
        {
          "DSMessagingVersion",
          (object) 1.0
        },
        {
          "DSId",
          (object) "my-rtmps"
        }
      };
            parameters[index] = (object)commandMessage;
            int num1 = 1;
            int num2 = 0;
            Method method = new Method(methodName, parameters, num1 != 0, (CallStatus)num2);
            invokeAmf0_2.MethodCall = method;
            invokeAmf0_1.ConnectionParameters = (object)new AsObject()
      {
        {
          "pageUrl",
          (object) pageUrl
        },
        {
          "objectEncoding",
          (object) (double) this._objectEncoding
        },
        {
          "capabilities",
          (object) 239.0
        },
        {
          "audioCodecs",
          (object) 3575.0
        },
        {
          "flashVer",
          (object) "WIN 11,7,700,169"
        },
        {
          "swfUrl",
          (object) swfUrl
        },
        {
          "videoFunction",
          (object) 1.0
        },
        {
          "fpad",
          (object) false
        },
        {
          "videoCodecs",
          (object) 252.0
        },
        {
          "tcUrl",
          (object) tcUrl
        },
        {
          "app",
          (object) ""
        }
      };
            invokeAmf0_1.InvokeId = this.GetNextInvokeId();
            return (AsObject)(await this.QueueCommandAsTask((Command)invokeAmf0_1, 3, 0, false)).Body;
        }

        private async Task<AsObject> ReconnectInvokeAsync(string pageUrl, string swfUrl, string tcUrl)
        {
            InvokeAmf0 invokeAmf0_1 = new InvokeAmf0();
            InvokeAmf0 invokeAmf0_2 = invokeAmf0_1;
            string methodName = "connect";
            object[] parameters = new object[4] { (object)false, (object)this.ClientId, (object)this._reconnectData, null };
            int index = 3;
            CommandMessage commandMessage = new CommandMessage();
            commandMessage.Operation = CommandOperation.Login;
            string str1 = "";
            commandMessage.CorrelationId = str1;
            string str2 = Uuid.NewUuid();
            commandMessage.MessageId = str2;
            string str3 = "";
            commandMessage.Destination = str3;
            string reconnectData = this._reconnectData;
            commandMessage.Body = (object)reconnectData;
            commandMessage.Headers = new AsObject()
      {
        {
          "DSMessagingVersion",
          (object) 1.0
        },
        {
          "DSId",
          (object) "my-rtmps"
        }
      };
            parameters[index] = (object)commandMessage;
            int num1 = 1;
            int num2 = 0;
            Method method = new Method(methodName, parameters, num1 != 0, (CallStatus)num2);
            invokeAmf0_2.MethodCall = method;
            invokeAmf0_1.ConnectionParameters = (object)new AsObject()
      {
        {
          "pageUrl",
          (object) pageUrl
        },
        {
          "objectEncoding",
          (object) (double) this._objectEncoding
        },
        {
          "capabilities",
          (object) 239.0
        },
        {
          "audioCodecs",
          (object) 3575.0
        },
        {
          "flashVer",
          (object) "WIN 11,7,700,169"
        },
        {
          "swfUrl",
          (object) swfUrl
        },
        {
          "videoFunction",
          (object) 1.0
        },
        {
          "fpad",
          (object) false
        },
        {
          "videoCodecs",
          (object) 252.0
        },
        {
          "tcUrl",
          (object) tcUrl
        },
        {
          "app",
          (object) ""
        }
      };
            invokeAmf0_1.InvokeId = this.GetNextInvokeId();
            return (AsObject)(await this.QueueCommandAsTask((Command)invokeAmf0_1, 3, 0, false)).Body;
        }

        public async Task<bool> SubscribeAsync(string endpoint, string destination, string subtopic, string clientId)
        {
            CommandMessage commandMessage = new CommandMessage()
            {
                ClientId = clientId,
                CorrelationId = null,
                Operation = CommandOperation.Subscribe,
                Destination = destination,
                Headers = new AsObject()
            {
                {
                    "DSEndpoint",
                    (object) endpoint
                },
                {
                    "DSId",
                    (object) clientId
                },
                {
                    "DSSubtopic",
                    (object) subtopic
                }
            }
            };
            return await this.InvokeAsync<string>((string)null, (object)commandMessage) == "success";
        }

        public async Task<bool> UnsubscribeAsync(string endpoint, string destination, string subtopic, string clientId)
        {
            CommandMessage commandMessage = new CommandMessage()
            {
                ClientId = clientId,
                CorrelationId = null,
                Operation = CommandOperation.Unsubscribe,
                Destination = destination,
                Headers = new AsObject()
                {
                    {
                        "DSEndpoint",
                        (object) endpoint
                    },
                    {
                        "DSId",
                        (object) clientId
                    },
                    {
                        "DSSubtopic",
                        (object) subtopic
                    }
                }
            };
            return await this.InvokeAsync<string>((string)null, (object)commandMessage) == "success";
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            return await this.LoginAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", (object)username, (object)password))));
        }

        public async Task<bool> LoginAsync(string base64)
        {
            CommandMessage commandMessage = new CommandMessage();
            string clientId = this.ClientId;
            commandMessage.ClientId = clientId;
            string empty = string.Empty;
            commandMessage.Destination = empty;
            int num = 8;
            commandMessage.Operation = (CommandOperation)num;
            string str = base64;
            commandMessage.Body = (object)str;
            return await this.InvokeAsync<string>((string)null, (object)commandMessage) == "success";
        }

        public Task LogoutAsync()
        {
            CommandMessage commandMessage = new CommandMessage();
            string clientId = this.ClientId;
            commandMessage.ClientId = clientId;
            string empty = string.Empty;
            commandMessage.Destination = empty;
            int num = 9;
            commandMessage.Operation = (CommandOperation)num;
            return (Task)this.InvokeAsync<object>((string)null, (object)commandMessage);
        }

        public void SetChunkSize(int size)
        {
            this.WriteProtocolControlMessage((RtmpEvent)new ChunkSize(size));
        }

        public Task PingAsync()
        {
            CommandMessage commandMessage = new CommandMessage();
            string clientId = this.ClientId;
            commandMessage.ClientId = clientId;
            string empty = string.Empty;
            commandMessage.Destination = empty;
            int num = 5;
            commandMessage.Operation = (CommandOperation)num;
            return (Task)this.InvokeAsync<object>((string)null, (object)commandMessage);
        }

        private void WriteProtocolControlMessage(RtmpEvent @event)
        {
            this._writer.Queue(@event, 2, 0);
        }

        private int GetNextInvokeId()
        {
            return Interlocked.Increment(ref this._invokeId);
        }

        public async Task<AcknowledgeMessageExt> InvokeAckAsync(int invokeId, string method, CommandMessage arg)
        {
            InvokeAmf0 invokeAmf0 = new InvokeAmf0();
            invokeAmf0.MethodCall = new Method(method, new object[1] { (object)arg }, 1 != 0, CallStatus.Request);
            invokeAmf0.InvokeId = invokeId;
            return await this.QueueCommandAsTask((Command)invokeAmf0, 3, 0, true);
        }

        internal async Task<AcknowledgeMessageExt> InvokeAckAsync(int invokeId, RemotingMessage message)
        {
            InvokeAmf3 invokeAmf3 = new InvokeAmf3();
            invokeAmf3.InvokeId = invokeId;
            invokeAmf3.MethodCall = new Method((string)null, new object[1]
            {
        (object) message
            }, 1 != 0, CallStatus.Request);
            return await this.QueueCommandAsTask((Command)invokeAmf3, 3, 0, true);
        }

        private async Task<AcknowledgeMessageExt> ConnectInvokeAckAsync(string pageUrl, string swfUrl, string tcUrl)
        {
            InvokeAmf0 invokeAmf0_1 = new InvokeAmf0();
            InvokeAmf0 invokeAmf0_2 = invokeAmf0_1;
            string methodName = "connect";
            object[] parameters = new object[4] { (object)false, (object)"nil", (object)"", null };
            int index = 3;
            CommandMessage commandMessage = new CommandMessage();
            commandMessage.Operation = CommandOperation.ClientPing;
            string str1 = "";
            commandMessage.CorrelationId = str1;
            string str2 = Uuid.NewUuid();
            commandMessage.MessageId = str2;
            string str3 = "";
            commandMessage.Destination = str3;
            commandMessage.Headers = new AsObject()
      {
        {
          "DSMessagingVersion",
          (object) 1.0
        },
        {
          "DSId",
          (object) "my-rtmps"
        }
      };
            parameters[index] = (object)commandMessage;
            int num1 = 1;
            int num2 = 0;
            Method method = new Method(methodName, parameters, num1 != 0, (CallStatus)num2);
            invokeAmf0_2.MethodCall = method;
            invokeAmf0_1.ConnectionParameters = (object)new AsObject()
      {
        {
          "pageUrl",
          (object) pageUrl
        },
        {
          "objectEncoding",
          (object) (double) this._objectEncoding
        },
        {
          "capabilities",
          (object) 239.0
        },
        {
          "audioCodecs",
          (object) 3575.0
        },
        {
          "flashVer",
          (object) "WIN 11,7,700,169"
        },
        {
          "swfUrl",
          (object) swfUrl
        },
        {
          "videoFunction",
          (object) 1.0
        },
        {
          "fpad",
          (object) false
        },
        {
          "videoCodecs",
          (object) 252.0
        },
        {
          "tcUrl",
          (object) tcUrl
        },
        {
          "app",
          (object) ""
        }
      };
            invokeAmf0_1.InvokeId = this.GetNextInvokeId();
            return await this.QueueCommandAsTask((Command)invokeAmf0_1, 3, 0, false);
        }

        private async Task<AcknowledgeMessageExt> ReconnectInvokeAckAsync(string pageUrl, string swfUrl, string tcUrl)
        {
            InvokeAmf0 invokeAmf0_1 = new InvokeAmf0();
            InvokeAmf0 invokeAmf0_2 = invokeAmf0_1;
            string methodName = "connect";
            object[] parameters = new object[4] { (object)false, (object)this.ClientId, (object)this._reconnectData, null };
            int index = 3;
            CommandMessage commandMessage = new CommandMessage();
            commandMessage.Operation = CommandOperation.Login;
            string str1 = "";
            commandMessage.CorrelationId = str1;
            string str2 = Uuid.NewUuid();
            commandMessage.MessageId = str2;
            string str3 = "";
            commandMessage.Destination = str3;
            string reconnectData = this._reconnectData;
            commandMessage.Body = (object)reconnectData;
            commandMessage.Headers = new AsObject()
      {
        {
          "DSMessagingVersion",
          (object) 1.0
        },
        {
          "DSId",
          (object) "my-rtmps"
        }
      };
            parameters[index] = (object)commandMessage;
            int num1 = 1;
            int num2 = 0;
            Method method = new Method(methodName, parameters, num1 != 0, (CallStatus)num2);
            invokeAmf0_2.MethodCall = method;
            invokeAmf0_1.ConnectionParameters = (object)new AsObject()
      {
        {
          "pageUrl",
          (object) pageUrl
        },
        {
          "objectEncoding",
          (object) (double) this._objectEncoding
        },
        {
          "capabilities",
          (object) 239.0
        },
        {
          "audioCodecs",
          (object) 3575.0
        },
        {
          "flashVer",
          (object) "WIN 11,7,700,169"
        },
        {
          "swfUrl",
          (object) swfUrl
        },
        {
          "videoFunction",
          (object) 1.0
        },
        {
          "fpad",
          (object) false
        },
        {
          "videoCodecs",
          (object) 252.0
        },
        {
          "tcUrl",
          (object) tcUrl
        },
        {
          "app",
          (object) ""
        }
      };
            invokeAmf0_1.InvokeId = this.GetNextInvokeId();
            return await this.QueueCommandAsTask((Command)invokeAmf0_1, 3, 0, false);
        }

        public async Task<AcknowledgeMessageExt> SubscribeAckAsync(int invokeId, string endpoint, string destination, string subtopic, string clientId)
        {
            CommandMessage commandMessage = new CommandMessage()
            {
                ClientId = clientId,
                CorrelationId = null,
                Operation = CommandOperation.Subscribe,
                Destination = destination,
                Headers = new AsObject()
                {
                    {
                        "DSEndpoint",
                        (object) endpoint
                    },
                    {
                        "DSId",
                        (object) clientId
                    },
                    {
                        "DSSubtopic",
                        (object) subtopic
                    }
                }
            };
            return await this.InvokeAckAsync(invokeId, (string)null, commandMessage);
        }

        public async Task<AcknowledgeMessageExt> UnsubscribeAckAsync(int invokeId, string endpoint, string destination, string subtopic, string clientId)
        {
            CommandMessage commandMessage = new CommandMessage()
            {
                ClientId = clientId,
                CorrelationId = null,
                Operation = CommandOperation.Unsubscribe,
                Destination = destination,
                Headers = new AsObject()
                {
                    {
                        "DSEndpoint",
                        (object) endpoint
                    },
                    {
                        "DSId",
                        (object) clientId
                    },
                    {
                        "DSSubtopic",
                        (object) subtopic
                    }
                }
            };
            return await this.InvokeAckAsync(invokeId, (string)null, commandMessage);
        }

        public async Task<AcknowledgeMessageExt> LoginAckAsync(int invokeId, string username, string password)
        {
            CommandMessage commandMessage = new CommandMessage();
            string clientId = this.ClientId;
            commandMessage.ClientId = clientId;
            string empty = string.Empty;
            commandMessage.Destination = empty;
            int num = 8;
            commandMessage.Operation = (CommandOperation)num;
            string str = this._reconnectData = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", (object)username, (object)password)));
            commandMessage.Body = (object)str;
            return await this.InvokeAckAsync(invokeId, (string)null, commandMessage);
        }

        public async Task<AcknowledgeMessageExt> LoginAckAsync(int invokeId, string base64)
        {
            CommandMessage commandMessage = new CommandMessage();
            string clientId = this.ClientId;
            commandMessage.ClientId = clientId;
            string empty = string.Empty;
            commandMessage.Destination = empty;
            int num = 8;
            commandMessage.Operation = (CommandOperation)num;
            string str = base64;
            commandMessage.Body = (object)str;
            return await this.InvokeAckAsync(invokeId, (string)null, commandMessage);
        }

        public Task LogoutAckAsync(int invokeId)
        {
            CommandMessage commandMessage1 = new CommandMessage();
            string clientId = this.ClientId;
            commandMessage1.ClientId = clientId;
            string empty = string.Empty;
            commandMessage1.Destination = empty;
            int num = 9;
            commandMessage1.Operation = (CommandOperation)num;
            CommandMessage commandMessage2 = commandMessage1;
            return (Task)this.InvokeAckAsync(invokeId, (string)null, commandMessage2);
        }

        public Task PingAckAsync(int invokeId)
        {
            CommandMessage commandMessage1 = new CommandMessage();
            string clientId = this.ClientId;
            commandMessage1.ClientId = clientId;
            string empty = string.Empty;
            commandMessage1.Destination = empty;
            int num = 5;
            commandMessage1.Operation = (CommandOperation)num;
            CommandMessage commandMessage2 = commandMessage1;
            return (Task)this.InvokeAckAsync(invokeId, (string)null, commandMessage2);
        }

        private void WrapCallback(Action action)
        {
            try
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    // ISSUE: reference to a compiler-generated field
                    EventHandler<Exception> callbackException = this.CallbackException;
                    if (callbackException == null)
                        return;
                    Exception e = ex;
                    callbackException((object)this, e);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static Task<AcknowledgeMessageExt> CreateExceptedTask(Exception exception)
        {
            TaskCompletionSource<AcknowledgeMessageExt> completionSource = new TaskCompletionSource<AcknowledgeMessageExt>();
            Exception exception1 = exception;
            completionSource.SetException(exception1);
            return completionSource.Task;
        }
    }
}