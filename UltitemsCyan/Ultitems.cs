using BepInEx;

using R2API;

using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

using UltitemsCyan.Items.Tier1;
using UltitemsCyan.Items.Tier2;
using UltitemsCyan.Items.Tier3;
using UltitemsCyan.Items.Lunar;
using UltitemsCyan.Items.Void;
using UltitemsCyan.Items.Untiered;
using UltitemsCyan.Equipment;
using UltitemsCyan.Buffs;

using System.Linq;
//using HarmonyLib;

// Unused?
//using UnityEngine.ResourceManagement.ResourceProviders;
//using System.IO;
//using System.Runtime.InteropServices.ComTypes;
//using System.Reflection;
//using Unity.Audio;
//using R2API.Utils;
using BepInEx.Configuration;
//using System;
using UltitemsCyan.Items;
using RoR2.ExpansionManagement;

namespace UltitemsCyan
{
    // Dependencies for when downloading the mod
    // For various important item methods
    [BepInDependency(ItemAPI.PluginGUID)]
    // For using Tokens
    [BepInDependency(LanguageAPI.PluginGUID)]
    // For making giving stat changes
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    // For using custom prefabs
    [BepInDependency(PrefabAPI.PluginGUID)]

    [BepInDependency(DotAPI.PluginGUID)]

    //[BepInDependency(PrefabAPI.PluginGUID)]
    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // TODO: Check if I need this for my mod specifically 
    // [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]



    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class Ultitems : BaseUnityPlugin
    {
        public static float stageStartTime; // measured in seconds

        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !

        // * * * Multiplayer Testing command:
        // connect localhost:7777

        // * * * S P E E D:
        // dtzoom

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SporkySpig";
        public const string PluginName = "UltitemsCyan";
        public const string PluginVersion = "0.13.3";

        public const string PluginSuffix = "Alloyed! No Birthdays I promise... What is Happening?";

        private static ConfigFile UltitemsConfig { get; set; }

        public static List<ItemDef.Pair> CorruptionPairs = [];
        public static PluginInfo PInfo { get; private set; }
        public static ExpansionDef sotvDLC;

        public static Sprite mysterySprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
        public static GameObject mysteryPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

        /* TODO: Add Assets and Config File
        // assets
        public static AssetBundle resources;

        public static GameObject cardPrefab;
        public static GameObject smallPrefab;

        public static Vector3 scaleTo;

        // config file
        private static ConfigFile cfgFile;
        //*/

        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            PInfo = Info;

            //ConfigInit();
            UltitemsConfig = new ConfigFile(Paths.ConfigPath + "\\Ultitems_ConfigFile.cfg", true);

            // Init My Assets Class
            UltAssets.Init();

            // Init Generic Game Events for taking damage items
            GenericGameEvents.Init();

            // *** *** Add buffs to the game
            List<BuffBase> ultitemBuffs = [];
            ultitemBuffs.Add(new BirthdayBuff()); //ALLOYED
            ultitemBuffs.Add(new CrysotopeFlyingBuff());
            ultitemBuffs.Add(new EyeAwakeBuff());
            ultitemBuffs.Add(new EyeDrowsyBuff());
            ultitemBuffs.Add(new EyeSleepyBuff());
            ultitemBuffs.Add(new DreamSpeedBuff());
            ultitemBuffs.Add(new DownloadedBuff());
            ultitemBuffs.Add(new FrisbeeGlidingBuff());
            ultitemBuffs.Add(new OverclockedBuff());
            ultitemBuffs.Add(new RottingBuff()); //ALLOYED

            ultitemBuffs.Add(new SlipperyGrapeBuff()); // % TAKEDAMAGEPROCESS

            ////// ultitemBuffs.Add(new PeelBuff());
            ultitemBuffs.Add(new QuarkGravityBuff());
            ultitemBuffs.Add(new SuesTeethBuff());
            ultitemBuffs.Add(new SporkBleedBuff());
            ultitemBuffs.Add(new TaffyChewBuff());
            ultitemBuffs.Add(new TickCritBuff());
            ultitemBuffs.Add(new ZorseStarvingBuff()); //ALLOYED

            foreach (BuffBase newBuff in ultitemBuffs)
            {
                newBuff.Init();
            }

            // *** *** Add items to the game
            List<ItemBase> ultitemItems = [];

            // *** Untiered
            ultitemItems.Add(new CorrodingVaultConsumed());
            ultitemItems.Add(new InhabitedCoffinConsumed());
            ultitemItems.Add(new SuesMandiblesConsumed());
            ultitemItems.Add(new SilverThreadConsumed()); // % TAKEDAMAGEPROCESS
            //ultitemItems.Add(new GreySolvent()); // With Obsolute

            // *** White
            ultitemItems.Add(new CremeBrulee());
            ultitemItems.Add(new KoalaSticker()); // % TAKEDAMAGEPROCESS // Should be affter grape buff
            ultitemItems.Add(new ToyRobot());
            ultitemItems.Add(new FleaBag());
            ultitemItems.Add(new Frisbee());

            // *** Green
            ultitemItems.Add(new BirthdayCandles()); //ALLOYED
            // moved Last Priority // ultitemItems.Add(new DegreeScissors());
            ultitemItems.Add(new HMT()); //ALLOYED
            ultitemItems.Add(new OverclockedGPU());
            ultitemItems.Add(new TinyIgloo()); // % ILContext
            ultitemItems.Add(new XenonAmpoule());

            // *** Red
            ultitemItems.Add(new CorrodingVault()); // Has Consumed Item
            ultitemItems.Add(new Grapevine()); // % TAKEDAMAGEPROCESS
            //ultitemItems.Add(new PigsSpork()); // HealthComponent_UpdateLastHitTime
            ultitemItems.Add(new RockyTaffy());
            ultitemItems.Add(new SuesMandibles()); // Has Consumed Item
            ultitemItems.Add(new ViralEssence());

            // *** Lunar Items
            ultitemItems.Add(new DreamFuel());
            ultitemItems.Add(new UltravioletBulb());
            ////// ultitemItems.Add(new PowerChip());

            ultitemItems.Add(new SilverThread()); // % TAKEDAMAGEPROCESS // Has Consumed Item

            //ultitemItems.Add(new DelugedPail()); //ALLOYED

            // *** Equipments
            ultitemItems.Add(new IceCubes());
            ////// ultitemItems.Add(new JellyJail());
            ultitemItems.Add(new OrbitalQuark());
            ultitemItems.Add(new YieldSign());
            ultitemItems.Add(new YieldSignStop());

            // *** Lunar Equipment
            ultitemItems.Add(new Macroseismograph());
            ultitemItems.Add(new MacroseismographConsumed());
            ultitemItems.Add(new PotOfRegolith());
            //ultitemItems.Add(new Obsolute()); // Has Consumed Item (creates consumed items from other items)


            // *** Void Items
            ultitemItems.Add(new Crysotope());
            ultitemItems.Add(new DriedHam());
            ultitemItems.Add(new RottenBones()); //ALLOYED
            ////// ultitemItems.Add(new QuantumPeel());
            ultitemItems.Add(new DownloadedRAM());
            ultitemItems.Add(new JealousFoe());
            ultitemItems.Add(new InhabitedCoffin()); // Has Consumed Item
            ultitemItems.Add(new ZorsePill()); //ALLOYED

            // Last Priority
            ultitemItems.Add(new DegreeScissors()); // After Vault and Coffin to grab consumed items

            foreach (ItemBase newItem in ultitemItems)
            {
                newItem.Init(UltitemsConfig);
                // If a void item (which always transforms other items) then add to corruption pair list
                if (newItem.GetTransformItem)
                {
                    //Log.Warning("Adding Void Transformation to list!");
                    CorruptionPairs.Add(new()
                    {
                        itemDef1 = newItem.GetTransformItem,
                        itemDef2 = newItem.GetItemDef,
                    });
                }
            }

            // Add Hooks
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            On.RoR2.Items.ContagiousItemManager.Init += ContagiousItemManager_Init;

            Log.Warning("Ultitems Cyan Done: " + PluginVersion + " <- " + PluginSuffix);
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            stageStartTime = Run.instance.time;
            //Log.Warning("Ultitem Starts at: " + stageStartTime);
        }


        // Add Void Pairs
        public void ContagiousItemManager_Init(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            // Add ultiCorruptionPairs to base game corruption pairs
            List<ItemDef.Pair> voidPairs = [.. ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem]]; // Collection Expression?
            PrintPairList(CorruptionPairs);
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = voidPairs.Union(CorruptionPairs).ToArray();
            Log.Debug("End of Ultitems init");
            orig();
        }

        private void PrintPairList(List<ItemDef.Pair> list)
        {
            foreach (ItemDef.Pair pair in list)
            {
                Log.Debug(". " + pair.itemDef1.name + " -> " + pair.itemDef2.name);
            }
        }

        //Static class for ease of access

    }
}