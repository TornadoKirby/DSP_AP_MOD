using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;

namespace Archipelago
{
    public static class APState
    {
        public class LocationItem
        {
            public string Location;
            public int LocationID;
            public int LocationDSPTech;
            public string Item;
            public int PlayerID;
        }

        public class APWorldData
        {
            public List<LocationItem> Locations;
            public Dictionary<string, string> Player_Names;
            public Dictionary<string, int> Tech_Table;
            public string Mod_Name;
            public List<string> Allowed_Science_Packs;
            public Dictionary<int, List<int>> Tech_Tree_Layout_Prerequisites;
        } 

        public class APWorldInfo
        {
                public string version;
                public string title;
                public string author;
                public string homepage;
                public string description;
                public string dsp_version;
                public string name;
        }

        public enum State
        {
            Menu,
            InGame
        }

        public static int[] AP_VERSION = new int[] { 0, 3, 4 };
        public static APData ServerData = new APData();
        //public static Dictionary<long, TechType> ITEM_CODE_TO_TECHTYPE = new Dictionary<long, TechType>();
        //public static Dictionary<long, Location> LOCATIONS = new Dictionary<long, Location>();

        public static APWorldData Data;
        public static APWorldInfo Info;
        public static List<Item> Items;
        public static List<Tech> Techs;
        public static List<Recipe> Recipes;
        public static bool DeathLinkKilling = false; // indicates player is currently getting DeathLinked
        public static Dictionary<string, int> archipelago_indexes = new Dictionary<string, int>();
        public static float unlock_dequeue_timeout = 0.0f;
        public static List<string> message_queue = new List<string>();
        public static float message_dequeue_timeout = 0.0f;
        public static State state = State.Menu;
        public static bool Authenticated;
        public static string Goal = "launch";
        public static string GoalEvent = "";
        public static bool Silent = false;
        public static Thread TrackerProcessing;
        public static long TrackedLocationsCount = 0;
        public static long TrackedFishCount = 0;
        public static string TrackedFish = "";
        public static long TrackedLocation = -1;
        public static string TrackedLocationName;
        public static float TrackedDistance;
        public static float TrackedAngle;

        public static ArchipelagoSession Session;
        public static ArchipelagoUI ArchipelagoUI = null;

        public static Dictionary<string, long> Encyclopdia;

        public static void Init()
        {
            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var reader = File.OpenText(pluginfolder + "/data/data.json");
            var content = reader.ReadToEnd();
            reader.Close();
            Data = Newtonsoft.Json.JsonConvert.DeserializeObject<APWorldData>(content);
            /*foreach (var loc in Data.Locations)
            {
                Debug.Log("Imported location: " + loc.Location + " has " + loc.Item + " for Player " + Data.Player_Names[loc.PlayerID.ToString()]);
                //ITEM_CODE_TO_TECHTYPE[itemJson.Key] =
                //    (TechType)Enum.Parse(typeof(TechType), itemJson.Value);
            }*/

            reader = File.OpenText(pluginfolder + "/data/info.json");
            content = reader.ReadToEnd();
            reader.Close();
            Info = Newtonsoft.Json.JsonConvert.DeserializeObject<APWorldInfo>(content);
        }
        public static bool Connect()
        {
            if (Authenticated)
            {
                return true;
            }
            // Start the archipelago session.
            var url = ServerData.host_name;
            int port = 38281;
            if (url.Contains(":"))
            {
                var splits = url.Split(new char[] { ':' });
                url = splits[0];
                if (!int.TryParse(splits[1], out port)) port = 38281;
            }

            Session = ArchipelagoSessionFactory.CreateSession(url, port);
            Session.Socket.PacketReceived += Session_PacketReceived;
            Session.Socket.ErrorReceived += Session_ErrorReceived;
            Session.Socket.SocketClosed += Session_SocketClosed;
            
            LoginResult loginResult = Session.TryConnectAndLogin(
                "Dyson Sphere Program", 
                ServerData.slot_name,
                new System.Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
                ItemsHandlingFlags.AllItems, 
                null, 
                "",
                ServerData.password == "" ? null : ServerData.password);

            if (loginResult is LoginSuccessful loginSuccess)
            {
                Authenticated = true;
                state = State.InGame;
            }
            else if (loginResult is LoginFailure loginFailure)
            {
                Authenticated = false;
                //ErrorMessage.AddMessage("Connection Error: " + String.Join("\n", loginFailure.Errors));
                Debug.LogError(String.Join("\n", loginFailure.Errors));
                Session.Socket.Disconnect();
                Session = null;
            }
            return loginResult.Successful;
        }
        
        public static void Session_SocketClosed(WebSocketSharp.CloseEventArgs e)
        {
            message_queue.Add("Connection to Archipelago lost: " + e.Reason);
            Debug.LogError("Connection to Archipelago lost: " + e.Reason);
            Disconnect();
        }
        public static void Session_ErrorReceived(Exception e, string message)
        {
            Debug.LogError(message);
            if (e != null) Debug.LogError(e.ToString());
            Disconnect();
        }

        public static void Disconnect()
        {
            if (Session != null && Session.Socket != null)
            {
                Session.Socket.Disconnect();
            }
            Session = null;
            Authenticated = false;
            state = State.Menu;
        }
        
        public static void Session_PacketReceived(ArchipelagoPacketBase packet)
        {
            Debug.Log("Incoming Packet: " + packet.PacketType.ToString());
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.Print:
                {
                    if (!Silent)
                    {
                        var p = packet as PrintPacket;
                        message_queue.Add(p.Text);
                    }
                    break;
                }

                case ArchipelagoPacketType.PrintJSON:
                {
                    /*if (!Silent)
                    {
                        var p = packet as PrintJsonPacket;
                        string text = "";
                        foreach (var messagePart in p.Data)
                        {
                            switch (messagePart.Type)
                            {
                                case "player_id":
                                    text += int.TryParse(messagePart.Text, out var playerSlot)
                                        ? Session.Players.GetPlayerAlias(playerSlot) ?? $"Slot: {playerSlot}"
                                        : messagePart.Text;
                                    break;
                                case "item_id":
                                    text += int.TryParse(messagePart.Text, out var itemId)
                                        ? Session.Items.GetItemName(itemId) ?? $"Item: {itemId}"
                                        : messagePart.Text;
                                    break;
                                case "location_id":
                                    text += int.TryParse(messagePart.Text, out var locationId)
                                        ? Session.Locations.GetLocationNameFromId(locationId) ?? $"Location: {locationId}"
                                        : messagePart.Text;
                                    break;
                                default:
                                    text += messagePart.Text;
                                    break;
                            }
                        }
                        message_queue.Add(text);
                    }*/
                    break;
                }
            }
        }

        public static void checkLocation(long id)
        {
            ServerData.@checked.Add(id);
            Session.Locations.CompleteLocationChecks(id);
        }

        public static void unlock(int techID/*TechType techType*/)
        {
            Debug.LogWarning($"unlock: DPS: {techID} AP: {techID - Plugin.DSP_LOCATION_ID_BASE + Plugin.AP_LOCATION_ID_BASE}"); 
            GameMain.history.UnlockTech(techID);
            /*if (PDAScanner.IsFragment(techType))
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);

                PDAScanner.Entry entry;
                if (!PDAScanner.GetPartialEntryByKey(techType, out entry))
                {
                    MethodInfo methodAdd = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(TechType), typeof(int) }, null);
                    entry = (PDAScanner.Entry)methodAdd.Invoke(null, new object[] { techType, 0 });
                }

                if (entry != null)
                {
                    entry.unlocked++;

                    if (entry.unlocked >= entryData.totalFragments)
                    {
                        List<PDAScanner.Entry> partial = (List<PDAScanner.Entry>)(typeof(PDAScanner).GetField("partial", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                        HashSet<TechType> complete = (HashSet<TechType>)(typeof(PDAScanner).GetField("complete", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                        partial.Remove(entry);
                        complete.Add(entry.techType);

                        MethodInfo methodNotifyRemove = typeof(PDAScanner).GetMethod("NotifyRemove", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(PDAScanner.Entry) }, null);
                        methodNotifyRemove.Invoke(null, new object[] { entry });

                        MethodInfo methodUnlock = typeof(PDAScanner).GetMethod("Unlock", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(PDAScanner.EntryData), typeof(bool), typeof(bool), typeof(bool) }, null);
                        methodUnlock.Invoke(null, new object[] { entryData, true, false, true });
                    }
                    else
                    {
                        int totalFragments = entryData.totalFragments;
                        if (totalFragments > 1)
                        {
                            float num2 = (float)entry.unlocked / (float)totalFragments;
                            float arg = (float)Mathf.RoundToInt(num2 * 100f);
                            ErrorMessage.AddError(Language.main.GetFormat<string, float, int, int>("ScannerInstanceScanned", Language.main.Get(entry.techType.AsString(false)), arg, entry.unlocked, totalFragments));
                        }

                        MethodInfo methodNotifyProgress = typeof(PDAScanner).GetMethod("NotifyProgress", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(PDAScanner.Entry) }, null);
                        methodNotifyProgress.Invoke(null, new object[] { entry });
                    }
                }
            }
            else
            {
                // Blueprint
                KnownTech.Add(techType, true);
            }*/
        }
        
        public static void send_completion()
        {
            var statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            Session.Socket.SendPacket(statusUpdatePacket);
        }
    }
}