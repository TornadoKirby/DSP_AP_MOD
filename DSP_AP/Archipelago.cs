using HarmonyLib;
using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Archipelago.MultiClient.Net.Packets;
using System.Text;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Debug = UnityEngine.Debug;
using File = System.IO.File;

namespace Archipelago
{
    public class Tech
    {
        public string Name;
        public int ID;
        public List<string> Requires;
        public List<string> Ingredients;
        public List<int> IngredientsCount;
        public long HashNeeded;
        public List<string> Unlocks;

        public Tech(string name, int id, List<string> requires, List<string> ingredients, List<int> ingredientsCount, long hashNeeded, List<string> unlocks)
        {
            Name = name;
            ID = id;
            Requires = requires;
            Ingredients = ingredients;
            IngredientsCount = ingredientsCount;
            HashNeeded = hashNeeded;
            Unlocks = unlocks;
        }
    };

    public class Recipe
    {
        public string Name;
        public int ID;
        public List<string> Ingredients;
        public List<int> IngredientsCount;
        public List<string> Products;
        public List<int> ProductsCount;

        public Recipe(string name, int id, List<string> ingredients, List<int> ingredientsCount, List<string> products, List<int> productsCount)
        {
            Name = name;
            ID = id;
            Ingredients = ingredients;
            IngredientsCount = ingredientsCount;
            Products = products;
            ProductsCount = productsCount;
        }
    };

    public class Item
    {
        public string Name;
        public int ID;
        public int StackSize;

        public Item(string name, int id, int stackSize)
        {
            Name = name;
            ID = id;
            StackSize = stackSize;
        }
    };
    public class ArchipelagoUI : MonoBehaviour
    {
        void OnGUI()
        {
            string ap_ver = "Archipelago v" + APState.AP_VERSION[0] + "." + APState.AP_VERSION[1] + "." + APState.AP_VERSION[2];
            if (APState.Session != null)
            {
                GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Connected");
            }
            else
            {
                GUI.Label(new Rect(16, 16, 300, 20), ap_ver + " Status: Not Connected");
            }

            if ((APState.Session == null || !APState.Authenticated) && APState.state == APState.State.Menu)
            {
                GUI.Label(new Rect(16, 36, 150, 20), "Host: ");
                GUI.Label(new Rect(16, 56, 150, 20), "PlayerName: ");
                GUI.Label(new Rect(16, 76, 150, 20), "Password: ");

                APState.ServerData.host_name = GUI.TextField(new Rect(150 + 16 + 8, 36, 150, 20), APState.ServerData.host_name);
                APState.ServerData.slot_name = GUI.TextField(new Rect(150 + 16 + 8, 56, 150, 20), APState.ServerData.slot_name);
                APState.ServerData.password = GUI.TextField(new Rect(150 + 16 + 8, 76, 150, 20), APState.ServerData.password);

                if (GUI.Button(new Rect(16, 96, 100, 20), "Connect"))
                {
                    APState.Connect();
                }
            }
            else if (APState.state == APState.State.InGame && APState.Session != null /*&& Player.main != null*/)
            {
                GUI.Label(new Rect(16, 96, 1000, 20), "ServerData.index: " + APState.ServerData.index + " AllItemsReceived: " + APState.Session.Items.AllItemsReceived.Count);

                if (APState.TrackedLocation != -1)
                {
                    GUI.Label(new Rect(16, 36, 1000, 20), 
                        "Locations left: " +
                        APState.TrackedLocationsCount +
                        ". Closest is " + (long)APState.TrackedDistance + " m away, named " + 
                        APState.TrackedLocationName);
                    // TODO: find a way to display this
                    //GUI.Label(new Rect(16, 56, 1000, 20), 
                    //    APState.TrackedAngle.ToString());
                }

                if (APState.TrackedFishCount > 0)
                {
                    GUI.Label(new Rect(16, 56, 1000, 22), 
                        "Fish left: "+APState.TrackedFishCount + ". Such as: "+APState.TrackedFish);
                }
            }
        }

        /*private void Start()
        {
            RegisterCmds();
        }

        public void RegisterCmds()
        {
            DevConsole.RegisterConsoleCommand(this, "say", false, false);
            DevConsole.RegisterConsoleCommand(this, "silent", false, false);
            DevConsole.RegisterConsoleCommand(this, "deathlink", false, false);
        }

        private void OnConsoleCommand_say(NotificationCenter.Notification n)
        {
            string text = "";

            for (var i = 0; i < n.data.Count; i++)
            {
                text += (string)n.data[i];
                if (i < n.data.Count - 1) text += " ";
            }
            // Cannot type the '!' character in subnautica console, will use / instead and replace them
            text = text.Replace('/', '!');
            
            if (APState.Session != null && APState.Authenticated)
            {
                var packet = new SayPacket();
                packet.Text = text;
                APState.Session.Socket.SendPacket(packet);
            }
            else
            {
                Debug.Log("Can only 'say' while connected to Archipelago.");
                ErrorMessage.AddMessage("Can only 'say' while connected to Archipelago.");
            }
        }
        private void OnConsoleCommand_silent(NotificationCenter.Notification n)
        {
            APState.Silent = !APState.Silent;
            
            if (APState.Silent)
            {
                Debug.Log("Muted Archipelago chat.");
                ErrorMessage.AddMessage("Muted Archipelago chat.");
            }
            else
            {
                Debug.Log("Enabled Archipelago chat.");
                ErrorMessage.AddMessage("Enabled Archipelago chat.");
            }
        }
        private void OnConsoleCommand_deathlink(NotificationCenter.Notification n)
        {
            APState.ServerData.death_link = !APState.ServerData.death_link;
            APState.set_deathlink();
            
            if (APState.ServerData.death_link)
            {
                Debug.Log("Enabled DeathLink.");
                ErrorMessage.AddMessage("Enabled DeathLink.");
            }
            else
            {
                Debug.Log("Disabled DeathLink.");
                ErrorMessage.AddMessage("Disabled DeathLink.");
            }
        }*/
    }
/*
    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("LoadInitialInventoryAsync")]
    internal class MainGameController_LoadInitialInventoryAsync_Patch
    {
        [HarmonyPostfix]
        public static void GameReady()
        {
            // Make sure the say command is registered
            APState.ArchipelagoUI.RegisterCmds();
        }
    }*/
    /*
    [HarmonyPatch(typeof(SaveLoadManager.GameInfo))]
    [HarmonyPatch("SaveIntoCurrentSlot")]
    internal class GameInfo_SaveIntoCurrentSlot_Patch
    {
        [HarmonyPostfix]
        public static void SaveIntoCurrentSlot(SaveLoadManager.GameInfo info)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(APState.ServerData));
            Platform.IO.File.WriteAllBytes(Platform.IO.Path.Combine(SaveLoadManager.GetTemporarySavePath(), 
                "archipelago.json"), bytes);
        }
    }
    
    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("SetCurrentSlot")]
    internal class SaveLoadManager_SetCurrentSlot_Patch
    {
        [HarmonyPostfix]
        public static void LoadArchipelagoState(string _currentSlot)
        {
            var storage = PlatformUtils.main.GetServices().GetUserStorage() as UserStoragePC;
            var rawPath = storage.GetType().GetField("savePath",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(storage);
            var path = Platform.IO.Path.Combine((string)rawPath, _currentSlot);

            path = Platform.IO.Path.Combine(path, "archipelago.json");
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    APState.ServerData = JsonConvert.DeserializeObject<APData>(reader.ReadToEnd());
                    
                    if (APState.Connect() && APState.ServerData.@checked != null)
                    {
                        APState.Session.Locations.CompleteLocationChecks(APState.ServerData.@checked.ToArray());
                    }
                    else
                    {
                        ErrorMessage.AddError("Null Checked");
                    }
                }
            }
            // compat handling, remove later
            else if (APState.archipelago_indexes.ContainsKey(_currentSlot))
            {
                APState.ServerData.index = APState.archipelago_indexes[_currentSlot];
            }
            else
            {
                APState.ServerData.index = 0;
            }
        }
    }*/
/*
    [HarmonyPatch(typeof(MainGameController))]
    [HarmonyPatch("OnDestroy")]
    internal class MainGameController_OnDestroy_Patch
    {
        [HarmonyPostfix]
        public static void GameClosing()
        {
            APState.state = APState.State.Menu;
        }
    }

    [HarmonyPatch(typeof(SaveLoadManager))]
    [HarmonyPatch("RegisterSaveGame")]
    internal class SaveLoadManager_RegisterSaveGame_Patch
    {
        [HarmonyPrefix]
        public static void RegisterSaveGame(string slotName, UserStorageUtils.LoadOperation loadOperation)
        {
            if (loadOperation.GetSuccessful())
            {
                byte[] jsonData = null;
                if (loadOperation.files.TryGetValue("gameinfo.json", out jsonData))
                {
                    try
                    {
                        var json_string = Encoding.UTF8.GetString(jsonData);
                        var splits = json_string.Split(new char[] { ',' });
                        var last = splits[splits.Length - 1];
                        splits = last.Split(new char[] { ':' });
                        var name = splits[0];
                        name = name.Substring(1, name.Length - 2);
                        splits = splits[1].Split(new char[] { '}' });
                        var value = splits[0];

                        if (name == "archipelago_item_index")
                        {
                            var index = int.Parse(value);
                            APState.archipelago_indexes[slotName] = index;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("archipelago_item_index error: " + e.Message);
                    }
                }
            }
        }
    }*/

    [HarmonyPatch(typeof(GameMain))]
    [HarmonyPatch("Update")]
    internal class GameMain_Update_Patch
    {
        private static bool IsSafeToUnlock()
        {
            if (APState.unlock_dequeue_timeout > 0.0f)
            {
                return false;
            }

            if (APState.state != APState.State.InGame)
            {
                return false;
            }

            /*if (IntroVignette.isIntroActive || LaunchRocket.isLaunching)
            {
                return false;
            }

            if (PlayerCinematicController.cinematicModeCount > 0 && Time.time - PlayerCinematicController.cinematicActivityStart <= 30f)
            {
                return false;
            }*/

            return true;//!SaveLoadManager.main.isSaving;
        }

        [HarmonyPostfix]
        public static void DequeueUnlocks()
        {
            const int DEQUEUE_COUNT = 2;
            const float DEQUEUE_TIME = 3.0f;

            if (APState.unlock_dequeue_timeout > 0.0f) APState.unlock_dequeue_timeout -= Time.deltaTime;
            if (APState.message_dequeue_timeout > 0.0f) APState.message_dequeue_timeout -= Time.deltaTime;

            // Print messages
            if (APState.message_dequeue_timeout <= 0.0f)
            {
                // We only do x at a time. To not crowd the on screen log/events too fast
                List<string> to_process = new List<string>();
                while (to_process.Count < DEQUEUE_COUNT && APState.message_queue.Count > 0)
                {
                    to_process.Add(APState.message_queue[0]);
                    APState.message_queue.RemoveAt(0);
                }
                foreach (var message in to_process)
                {
                    //ErrorMessage.AddMessage(message);
                }
                APState.message_dequeue_timeout = DEQUEUE_TIME;
            }

            // Do unlocks
            if (IsSafeToUnlock())
            {
                if (APState.ServerData.index < APState.Session.Items.AllItemsReceived.Count)
                {
                    APState.unlock(APState.Session.Items.AllItemsReceived[Convert.ToInt32(APState.ServerData.index)].Item - Plugin.AP_LOCATION_ID_BASE + Plugin.DSP_LOCATION_ID_BASE);
                    APState.ServerData.index++;
                    // We only do x at a time. To not crowd the on screen log/events too fast
                    APState.unlock_dequeue_timeout = DEQUEUE_TIME;
                }
            }
        }
    }

    [HarmonyPatch(typeof(UIMainMenu), "_OnCreate")]
    internal class UIMainMenu__OnCreate_Patch
    {

        private static void DumpTechInfo(string param)
        {
            List<Recipe> Recipes = new List<Recipe>();
            foreach (int i in Enumerable.Range(0, LDB.recipes.dataArray.Length))
            {
                var recipe = LDB.recipes.dataArray[i];
                Recipes.Add(new Recipe( recipe.Name.Translate(Language.enUS), 
                                        recipe.ID, 
                                        recipe.Items.Select(item => LDB.items.Select(item).Name.Translate(Language.enUS)).ToList(),
                                        recipe.ItemCounts.ToList(),
                                        recipe.Results.Select(item => LDB.items.Select(item).Name.Translate(Language.enUS)).ToList(),
                                        recipe.ResultCounts.ToList()));
            }
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(Recipes);
            var bytes = Encoding.UTF8.GetBytes(data);
            File.WriteAllBytes($"{param}/recipes.json", bytes);
            Debug.Log("Exported Tech Data"); 
        }

        private static void DumpRecipeInfo(string param)
        {
            List<Tech> Techs = new List<Tech>();
            foreach (int i in Enumerable.Range(0, LDB.techs.dataArray.Length))
            {
                var tech = LDB.techs.dataArray[i];
                Techs.Add(new Tech( tech.nameAndLevel.Translate(Language.enUS), 
                                    tech.ID, 
                                    tech.PreTechs.Select(preTech => LDB.techs.Select(preTech).nameAndLevel.Translate(Language.enUS)).ToList(), 
                                    tech.Items.Select(item => LDB.items.Select(item).Name.Translate(Language.enUS)).ToList(),
                                    tech.ItemPoints.ToList(),
                                    tech.HashNeeded,
                                    tech.UnlockRecipes.Select(recipe => LDB.recipes.Select(recipe).Name.Translate(Language.enUS)).ToList()));
            }
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(Techs);
            var bytes = Encoding.UTF8.GetBytes(data);
            File.WriteAllBytes($"{param}/techs.json", bytes);
            Debug.Log("Exported Recipe Data"); 
        }

        private static void DumpItemInfo(string param)
        {
            List<Item> Items = new List<Item>();
            foreach (int i in Enumerable.Range(0, LDB.items.dataArray.Length))
            {
                var item = LDB.items.dataArray[i];
                Items.Add(new Item( item.Name.Translate(Language.enUS), 
                                    item.ID, 
                                    item.StackSize));
            }
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(Items);
            var bytes = Encoding.UTF8.GetBytes(data);
            File.WriteAllBytes($"{param}/items.json", bytes);
            Debug.Log("Exported Item Data"); 
        }

        private static string DumpGameInfo(string param)
        {
            param = "E:\\Github\\DSP_AP_Mod\\DSP_AP\\data";
            Directory.CreateDirectory(param);
            DumpTechInfo(param);
            DumpRecipeInfo(param);
            DumpItemInfo(param);

            return "Info dumped";
        }

        [HarmonyPostfix]
        public static void CreateArchipelagoUI()
        {
            // Create a game object that will be responsible to drawing the IMGUI in the Menu.
            var guiGameobject = new GameObject();
            APState.ArchipelagoUI = guiGameobject.AddComponent<ArchipelagoUI>();
            GameObject.DontDestroyOnLoad(guiGameobject);
            Debug.Log($"ArchipelagoUI created");

            XConsole.RegisterCommand("ap-get-info-dump", (string param) => DumpGameInfo(param));
        }
    }
 /*
    [HarmonyPatch(typeof(PlayerAction_Mine), "AddProductionStat")]
    internal class TestUnlock_Patch
    {
        [HarmonyPostfix]
        public static void AddProductionStat_Patch()
        {
            Debug.LogWarning($"AddProductionStat_Patch begin");
            GameMain.history.UnlockTech(1144);
            Debug.LogWarning($"AddProductionStat_Patch end");
        }
    }*/

    [HarmonyPatch(typeof(ADV_UnlockTech), "OnUnlockTech")]
    internal class ADV_UnlockTech_Patch
    {
        [HarmonyPostfix]
        public static void OnUnlockTech_Patch(int techId)
        {
            Debug.LogWarning($"OnUnlockTech_Patch begin");
            int tech = APState.Data.Locations.Find(x => x.LocationDSPTech == techId).LocationID;
            Debug.LogWarning($"OnUnlockTech: DPS: {tech} AP: {tech - Plugin.DSP_LOCATION_ID_BASE + Plugin.AP_LOCATION_ID_BASE}");
            APState.checkLocation(tech - Plugin.DSP_LOCATION_ID_BASE + Plugin.AP_LOCATION_ID_BASE);
            Debug.LogWarning($"OnUnlockTech_Patch end");
        }
    }
}