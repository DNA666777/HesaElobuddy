﻿using AutoShop.Controllers;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using System;

namespace AutoShop
{
    internal class AutoShop
    {
        public static Menu ShopMenu;
        static bool gameLoaded = false;

        public AutoShop()
        {
            InitializeMenu();
            InitializeEvents();
            Chat.OnInput += Chat_OnInput;
            Loading.OnLoadingComplete += delegate
            {
                gameLoaded = true;
            };
        }

        private static void InitializeMenu()
        {
            ShopMenu = MainMenu.AddMenu("AutoShop", "hesa_autoshop", "AutoShop");
            ShopMenu.AddGroupLabel("AutoShop allow you to buy items from your custom build when you visit the shop.");
            ShopMenu.AddLabel("Any and all suggestions are welcome.");
            ShopMenu.AddSeparator(400);
            ShopMenu.AddGroupLabel("Use '/b itemname' command to add an item to this list.");
            ShopMenu.AddGroupLabel("Use '/s itemname' command to sell an item from this list.");
        }

        private static void InitializeEvents()
        {
            Game.OnTick += OnTick;
            Drawing.OnEndScene += Drawings;
        }
        static bool isDelayed = false;
        private static void OnTick(EventArgs args)
        {
            if (!gameLoaded || isDelayed) return;
            //Core.DelayAction(() =>
            //{
            isDelayed = true;
            if (IsInShop())
            {
                //Build
                BuildController.BuyOrSellItems();
                //Potions
                PotionController.BuyOrSellPotions();
            }
            isDelayed = false;
            //}, 100);

        }

        public static bool IsInShop()
        {
            return Shop.CanShop || ObjectManager.Player.IsInShopRange();
        }

        private void Chat_OnInput(ChatInputEventArgs args)
        {

        }

        private static void Drawings(EventArgs args)
        {

        }

        public static bool Recalling()
        {
            return ObjectManager.Player.IsRecalling();
        }
    }
}