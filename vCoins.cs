﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server.Plugin;
using Terraria_Server;
using Terraria_Server.Events;
using Terraria_Server.Logging;
using Terraria_Server.Commands;
using System.Xml;
using System.IO;

namespace vCoins
{
    public class vCoins : Plugin
    {
        public bool isEnabled = true;
        public bool first = true;
        public int userbalance = 0;
        public int userbalancebak = 0;
        public int balancetogive = 0;
        public int targetbalance = 0;
        string user;
        string userbak;
        string targetuser;
        string pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + "vCoins";
        string playerFolder = Statics.PluginPath + Path.DirectorySeparatorChar + "vCoins" + Path.DirectorySeparatorChar + "Players" + Path.DirectorySeparatorChar;

        public override void Load()
        {
            Name = "vCoins";
            Description = "Basic";
            Author = "The Prodigy";
            Version = "1";
            TDSMBuild = 31;

            string pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + "vCoins";
            //Create folder if it doesn't exist
            CreateDirectory(pluginFolder);
            

            AddCommand("vcoins")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("Shows the commands for vCoins.")
                .WithHelpText("/vcoins")
                .Calls(vCommands);
            AddCommand("vbalance")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("Shows your vCoins balance.")
                .WithHelpText("/vbalance")
                .Calls(vBalance);
            AddCommand("vgive")
                .WithAccessLevel(AccessLevel.PLAYER)
                .WithDescription("Gives said player said amount of vCoins.")
                .WithHelpText("/vgive name number")
                .Calls(vGive);
            AddCommand("vset")
                .WithAccessLevel(AccessLevel.OP)
                .WithDescription("Sets said player to said vCoin balance.")
                .WithHelpText("/vset name number")
                .Calls(vSet);
                
        }

        public override void Enable()
        {
            Console.WriteLine("[VCOINS] Ready and Waiting!");
            isEnabled = true;
            this.registerHook(Hooks.PLAYER_LOGIN);
        }
        public override void Disable()
        {
            Console.WriteLine("[VCOINS] Powering Down!");
            isEnabled = false;
        }
            public void vCommands(Server server, ISender sender, ArgumentList args)
            {
                sender.sendMessage("vCoins commands:");
                sender.sendMessage("/vbalance - Shows your vCoins balance.");
                sender.sendMessage("/vgive name number - Gives said player said amount of vCoins.");
                if (sender.Op == true)
                {
                    sender.sendMessage("/vset name number - Sets said player to said vCoin balance.");
                }
            }
            public void vBalance(Server server, ISender sender, ArgumentList args)
            {
                user = sender.Name.ToString().Replace(" ", "").ToLower();
                FileStream dataread = new FileStream(playerFolder + user + ".dat", FileMode.Open);
                BinaryReader datareader = new BinaryReader(dataread);
                userbalance = datareader.ReadInt32();
                dataread.Close();
                datareader.Close();
                sender.sendMessage("Your current vCoins balance is: " + userbalance + ".");
            }
            public void vGive(Server server, ISender sender, ArgumentList args)
            {
                try
                {
                    targetuser = args[0].ToString().ToLower();
                    balancetogive = Convert.ToInt32(args[1]);

                    user = sender.Name.ToString().Replace(" ", "").ToLower();
                    FileStream dataread = new FileStream(playerFolder + user + ".dat", FileMode.Open);
                    BinaryReader datareader = new BinaryReader(dataread);
                    userbalance = datareader.ReadInt32();
                    dataread.Close();
                    datareader.Close();
                    userbalance = (userbalance - balancetogive);
                    if (userbalance <0)
                    {
                        sender.sendMessage("You don't have enough vCoins for that!");
                        userbalance = (userbalance + balancetogive);
                            goto notenough;
                    }
                    FileStream datawrite = new FileStream(playerFolder + user + ".dat", FileMode.Create, FileAccess.ReadWrite);
                    BinaryWriter datawriter = new BinaryWriter(datawrite);
                    datawriter.Write(userbalance);
                    datawrite.Close();
                    datawriter.Close();

                    FileStream datareadtarget = new FileStream(playerFolder + targetuser + ".dat", FileMode.Open);
                    BinaryReader datareadertarget = new BinaryReader(datareadtarget);
                    targetbalance = datareadertarget.ReadInt32();
                    datareadtarget.Close();
                    datareadertarget.Close();
                    targetbalance = (targetbalance + balancetogive);
                    FileStream datawritetarget = new FileStream(playerFolder + targetuser + ".dat", FileMode.Create, FileAccess.ReadWrite);
                    BinaryWriter datawritertarget = new BinaryWriter(datawritetarget);
                    datawritertarget.Write(targetbalance);
                    datawritetarget.Close();
                    datawritertarget.Close();
                    sender.sendMessage("You gave " + targetuser + " " + balancetogive + " vCoins.");
                    notenough: ;
                }
                catch
                {
                    sender.sendMessage("Command error!");
                    sender.sendMessage("Example: /vgive samual 100");
                }

            }
            public void vSet(Server server, ISender sender, ArgumentList args)
            {
                try
                {
                    userbak = user;
                    userbalancebak = userbalance;
                    user = args[0].ToString().ToLower();
                    userbalance = Convert.ToInt32(args[1]);
                    FileStream datawrite = new FileStream(playerFolder + user + ".dat", FileMode.Create, FileAccess.ReadWrite);
                    BinaryWriter datawriter = new BinaryWriter(datawrite);
                    datawriter.Write(userbalance);
                    datawrite.Close();
                    datawriter.Close();
                    sender.sendMessage(user + "'s vCoins balance was set to " + userbalance + ".");
                    if (user != userbak)
                    {
                        user = userbak;
                        userbalance = userbalancebak;
                    }
                }
                catch
                {
                    sender.sendMessage("Command error!");
                    sender.sendMessage("Example: /vset samual 200");
                }
            }

            public override void onPlayerJoin(PlayerLoginEvent Event)
            {
                try
                {
                    user = Event.Sender.Name.ToString().Replace(" ", "").ToLower();
                    FileStream dataread = new FileStream(playerFolder + user + ".dat", FileMode.Open);
                    BinaryReader datareader = new BinaryReader(dataread);
                    userbalance = datareader.ReadInt32();
                    dataread.Close();
                    datareader.Close();
                }
                catch
                {
                    userbalance = 0;
                    user = Event.Sender.Name.ToString().Replace(" ", "").ToLower();
                    string usernew = Event.Player.Name.ToString();
                    NetMessage.SendData(25, -1, -1, "New vCoin account created for " + usernew + ".", 255, 0, 255, 255);
                    FileStream datawrite = new FileStream(playerFolder + user + ".dat", FileMode.Create, FileAccess.ReadWrite);
                    BinaryWriter datawriter = new BinaryWriter(datawrite);
                    datawriter.Write(userbalance);
                    datawrite.Close();
                    datawriter.Close();
                }

            }

        private static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}
