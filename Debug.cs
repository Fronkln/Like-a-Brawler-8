﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonEngineLibrary;
using DragonEngineLibrary.Advanced;
using System.Runtime.InteropServices;

namespace LikeABrawler2
{
    internal static unsafe class Debug
    {
        public static bool NoUI = false;
        public static bool AttackFirstMember;

        public static void GameUpdate()
        {
        }
        public static void InputUpdate()
        {
            if (DragonEngine.IsKeyHeld(VirtualKey.LeftShift))
            {
                if (DragonEngine.IsKeyDown(VirtualKey.P))
                {
                    DragonEngine.Log("Misc debug key");
                    DragonEngine.Log(TimelineManager.CheckClockAchievement(53, 27, 6));
                }

                if (DragonEngine.IsKeyDown(VirtualKey.C))
                {
                    AttackFirstMember = !AttackFirstMember;
                    DragonEngine.Log("Attack First Member: " + AttackFirstMember);
                }

                if (DragonEngine.IsKeyDown(VirtualKey.M))
                {
                    if (Mod.IsTurnBased())
                        BrawlerBattleManager.ChangeToRealtime();
                    else
                        BrawlerBattleManager.ChangeToTurnBased();

                    DragonEngine.Log("Is turn based: " + Mod.IsTurnBased());
                }

                //  Mod.ReloadContent();

                if (DragonEngine.IsKeyDown(VirtualKey.G))
                {
                    Mod.ReloadContent();
                    DragonEngine.Log("Reload mod content");
                }

                if (DragonEngine.IsKeyDown(VirtualKey.F))
                {
                    BrawlerPlayer.GodMode = !BrawlerPlayer.GodMode;
                    DragonEngine.Log("Player God Mode:" + BrawlerPlayer.GodMode);
                }

                if (DragonEngine.IsKeyHeld(VirtualKey.X))
                {
                    NoUI = !NoUI;
                    DragonEngine.Log("No UI: " + NoUI);
                }

                if (DragonEngine.IsKeyHeld(VirtualKey.Numpad7))
                {
                    BrawlerBattleManager.ForceGivePlayerTurn();
                    BrawlerBattleManager.SkipTurn();

                    DragonEngine.Log("player turn");
                }

                if(DragonEngine.IsKeyHeld(VirtualKey.J))
                {
                    DragonEngine.Log("0 damage");

                    for (uint i = 0; i < 4; i++)
                        FighterManager.GetFighter(i).GetStatus().AttackPower = 0;
                }
                if (DragonEngine.IsKeyDown(VirtualKey.Numpad8))
                {
                    DragonEngine.Log("300 damage");
                    BrawlerBattleManager.PlayerFighter.GetStatus().AttackPower = 300;
                }


                if (DragonEngine.IsKeyDown(VirtualKey.Numpad9))
                {
                    DragonEngine.Log("5000 damage");
                    BrawlerBattleManager.PlayerFighter.GetStatus().AttackPower = 5000;
                }
            }
        }
    }
}
