using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using CommonAPI;
using CommonAPI.Systems;


namespace Archipelago
{
    [BepInPlugin("dsp.archipelago", "archipelago mod", "1.0.0")]
    [BepInDependency(CommonAPIPlugin.GUID)]
	[CommonAPISubmoduleDependency(nameof(ProtoRegistry))]
    [BepInProcess("DSPGAME.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const int AP_LOCATION_ID_BASE = 86000;
        public const int DSP_LOCATION_ID_BASE = 500;

        List<int> TechTreeDepth = new List<int>();
        Dictionary<int, Tuple<Vector2, int>> TechPositions = new Dictionary<int, Tuple<Vector2, int>>();

        private int PrereqDepth(int tech)
        {
            if (tech == 1 || !APState.Data.Tech_Tree_Layout_Prerequisites.TryGetValue(tech, out List<int> prereq))
            {
                return 0;
            }
            
            int currentDepth = 0;
            foreach (int pre in prereq)
            {
                int depth = PrereqDepth(pre);
                if (depth > currentDepth)
                {
                    currentDepth = depth;
                }
            }
            return currentDepth + 1;
        }
        private void FindTechPosition(int tech)
        {
            int depth = PrereqDepth(tech);
            
            if (TechTreeDepth.Count <= depth)
            {
                TechTreeDepth.AddRange(Enumerable.Repeat(0, depth - TechTreeDepth.Count + 1));
            }
            TechTreeDepth[depth] += 1;
            Logger.LogInfo($"GetPosition: {depth} {TechTreeDepth[depth]}");
            TechPositions[tech] = new Tuple<Vector2, int>(new Vector2(10 + depth * 6 + TechTreeDepth[depth] * 0.1f, 5 + TechTreeDepth[depth] * 6 + depth * 0.1f), depth);
        }

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Archipelago mod 1.0.0 is loaded!");

            APState.Init();

            using (ProtoRegistry.StartModLoad("dsp.archipelago"))
            {
                ProtoRegistry.RegisterString("APProgressionItem", "Archipelago progression item");
                ProtoRegistry.RegisterString("APProgressionItemDesc", "This is a progression item for someone.");
                ProtoRegistry.RegisterString("APJunkItem", "Archipelago junk item");
                ProtoRegistry.RegisterString("APJunkItemDesc", "This is a junk item for someone.");

                // hide real techs (those will get received)
                for (int i=0;i<LDB.techs.dataArray.Length;i++)
                {
                    if (LDB.techs.dataArray[i].ID != 1)
                    {
                        //LDB.techs.dataArray[i].Published = false;
                        LDB.techs.dataArray[i].Position = new Vector2(-1000, 0);
                    }
                }
                int playerID = Int32.Parse(APState.Info.name.Split('-')[2].Remove(0, 1));
                Logger.LogInfo($"playerID: {playerID}");
                int count = 1;

                foreach (var loc in APState.Data.Locations)
                {
                    FindTechPosition(loc.LocationDSPTech);
                }

                Vector2[] TechsPositionOffset = TechTreeDepth.Select(depth => new Vector2(0, -depth * 6 / 2.0f)).ToArray();
                
                foreach (var loc in APState.Data.Locations)
                {
                    Logger.LogInfo($"Creating new Location from source: {loc.LocationID}");
                    var sourceTech = LDB.techs.Select(loc.LocationID);
                    //if (!loc.Location.Contains("level"))
                    {
                        if (!APState.Data.Tech_Tree_Layout_Prerequisites.TryGetValue(loc.LocationDSPTech, out List<int> prerequites))
                        {
                            prerequites = new List<int> {1};
                        }                       

                        ProtoRegistry.RegisterString(loc.Location, loc.Item);
                        ProtoRegistry.RegisterString(loc.Location + " Desc", "Send " + loc.Item + " to " + APState.Data.Player_Names[loc.PlayerID.ToString()]);
                        ProtoRegistry.RegisterString(loc.Location + " Conc", "Sent " + loc.Item + " to " + APState.Data.Player_Names[loc.PlayerID.ToString()]);
                        Logger.LogInfo($"Creating new Location:  {APState.Data.Tech_Table[loc.Location] - AP_LOCATION_ID_BASE + DSP_LOCATION_ID_BASE} {loc.Location} {count++ * 3} {(loc.PlayerID == playerID ? 3 : -3)}");
                        Logger.LogInfo($"{loc.LocationDSPTech}");
                        Logger.LogInfo($"{loc.Location}");
                        Logger.LogInfo(loc.PlayerID == playerID ? LDB.techs.Select(APState.Data.Locations.First(x=>x.Location == loc.Item).LocationID).IconPath : "");
                        Logger.LogInfo($"{prerequites.ToArray()}");
                        Logger.LogInfo($"{sourceTech.Items}");
                        Logger.LogInfo($"{sourceTech.ItemPoints}");
                        Logger.LogInfo($"{sourceTech.HashNeeded}");
                        Logger.LogInfo($"{TechPositions[loc.LocationDSPTech].Item1 + TechsPositionOffset[TechPositions[loc.LocationDSPTech].Item2]}");
                        var netTech = ProtoRegistry.RegisterTech(
                            loc.LocationDSPTech,
                            loc.Location,
                            loc.Location + " Desc",
                            loc.Location + " Conc",
                            loc.PlayerID == playerID ? LDB.techs.Select(APState.Data.Locations.First(x=>x.Location == loc.Item).LocationID).IconPath : "",
                            prerequites.ToArray(), 
                            sourceTech.Items,
                            sourceTech.ItemPoints,
                            sourceTech.HashNeeded,
                            new int[] { 0 },
                            TechPositions[loc.LocationDSPTech].Item1 + TechsPositionOffset[TechPositions[loc.LocationDSPTech].Item2]);
                        Logger.LogInfo($"Created new Location");
                    }
                }

                //var netTech = ProtoRegistry.RegisterTech(1003, "APProgressionItem", "APProgressionItemDesc", "APProgressionItemDesc", ""/*LDB.techs.Select(1601).IconPath*/, new int[] { 1 }, new int[] { 1202 }, new int[] { 30 }, 1200, new int[] { 9 }, new Vector2(8, -3));
                //LDB.techs.Select(1601).PreTechs = new int[] {1003};
                //GameObject.FindObjectOfType<XConsole>().showConsole = true;
                //DSPGame.SkipPrologue = true;
                //LDB.techs.Select(1601).preTechArray = new TechProto[] {netTech};
                //ProtoRegistry.RegisterRecipe(5003, (ERecipeType)5, num12, new int[2] { 1121, 1123 }, new int[2] { num11, num10 }, new int[1] { 1101 }, new int[1] { 1 }, "IronFusionDesc", num6, 1413, "IronFusion", val3.IconPath);
            }

            var harmony = new Harmony("dsp.archipelago");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Patching done!");
        }
    }

    /*public static class TechProtoExtension
    {
        public static void ReplaceIcon(this TechProto proto, TechProto srcProto)
        {
            proto._iconSprite = srcProto._iconSprite;
        }
    }*/
}