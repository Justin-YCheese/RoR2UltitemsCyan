using UnityEngine;

namespace UltitemsCyan
{
    public static class UltAssets
    {
        //The mod's AssetBundle
        public static AssetBundle mainBundle;

        // White
        public static Sprite CremeBruleeSprite;
        public static Sprite FleaBagSprite;
        public static Sprite FrisbeeSprite;
        public static Sprite KoalaStickerSprite;
        public static Sprite ToyRobotSprite;
        public static GameObject CremeBruleePrefab;
        public static GameObject FleaBagPrefab;
        public static GameObject FrisbeePrefab;
        public static GameObject KoalaStickerPrefab;
        public static GameObject ToyRobotPrefab;

        // Green
        public static Sprite BirthdayCandleSprite;
        public static Sprite DegreeScissorsSprite;
        public static Sprite HMTSprite;
        public static Sprite OverclockedGPUSprite;
        public static Sprite TinyIglooSprite;
        //public static Sprite TippedArrowSprite;
        public static Sprite XenonAmpouleSprite;
        public static GameObject BirthdayCandlePrefab;
        public static GameObject DegreeScissorsPrefab;
        public static GameObject HMTPrefab;
        public static GameObject OverclockedGPUPrefab;
        public static GameObject TinyIglooPrefab;
        //public static GameObject TippedArrowPrefab;
        public static GameObject XenonAmpoulePrefab;

        // Red
        public static Sprite CorrodingVaultSprite;
        //public static Sprite FaultyBulbSprite;
        public static Sprite GrapevineSprite;
        public static Sprite PigsSporkSprite;
        public static Sprite RockyTaffySprite;
        public static Sprite SuesMandiblesSprite;
        public static Sprite ViralSmogSprite;
        public static GameObject CorrodingVaultPrefab;
        //public static GameObject FaultyBulbPrefab;
        public static GameObject GrapevinePrefab;
        public static GameObject PigsSporkPrefab;
        public static GameObject RockyTaffyPrefab;
        public static GameObject SuesMandiblesPrefab;
        public static GameObject ViralSmogPrefab;


        // Void
        public static Sprite CrysotopeSprite;
        public static Sprite DownloadedRAMSprite;
        public static Sprite DriedHamSprite;
        public static Sprite InhabitedCoffinSprite;
        public static Sprite JubilantFoeSprite;
        public static Sprite ResinWhirlpoolSprite;
        public static Sprite RottenBonesSprite;
        //public static Sprite TungstenRodSprite;
        //public static Sprite WormHolesSprite;
        public static Sprite ZorsePillSprite;
        public static GameObject DownloadedRAMPrefab;
        public static GameObject CrysotopePrefab;
        public static GameObject DriedHamPrefab;
        public static GameObject InhabitedCoffinPrefab;
        public static GameObject JubilantFoePrefab;
        public static GameObject ResinWhirlpoolPrefab;
        public static GameObject RottenBonesPrefab;
        //public static GameObject TungstenRodPrefab;
        //public static GameObject WormHolesPrefab;
        public static GameObject ZorsePillPrefab;

        // Lunar
        //public static Sprite CreatureDeckSprite;
        public static Sprite DreamFuelSprite;
        public static Sprite UltravioletBulbSprite;
        //public static Sprite PowerChipsSprite;
        public static Sprite SandPailSprite;
        public static Sprite SilverThreadSprite;
        //public static GameObject CreatureDeckPrefab;
        public static GameObject UltravioletBulbPrefab;
        public static GameObject DreamFuelPrefab;
        //public static GameObject PowerChipsPrefab;
        public static GameObject SandPailPrefab;
        public static GameObject SilverThreadPrefab;

        // Untiered
        public static Sprite CorrodingVaultConsumedSprite;
        public static Sprite InhabitedCoffinConsumedSprite;
        public static Sprite SuesMandiblesConsumedSprite;
        public static Sprite SilverThreadConsumedSprite;
        public static Sprite UniversalSolventSprite;
        public static GameObject CorrodingVaultConsumedPrefab;
        public static GameObject InhabitedCoffinConsumedPrefab;
        public static GameObject SuesMandiblesConsumedPrefab;
        public static GameObject SilverThreadConsumedPrefab;
        public static GameObject UniversalSolventPrefab;

        // Equipment
        //public static Sprite JellyJailSprite;
        public static Sprite IceCubesSprite;
        public static Sprite OrbitalQuarkSprite;
        public static Sprite YieldSignSprite;
        public static Sprite YieldSignStopSprite;
        //public static Sprite PetRockSprite;
        //public static Sprite TrebuchetSprite;
        //public static GameObject JellyJailPrefab;
        public static GameObject IceCubesPrefab;
        public static GameObject OrbitalQuarkPrefab;
        public static GameObject YieldSignPrefab;
        public static GameObject YieldSignStopPrefab;

        // Lunar Equipment
        public static Sprite MacroseismographSprite;
        public static Sprite MacroseismographConsumedSprite;
        public static Sprite PotOfRegolithSprite;
        public static Sprite ObsoluteSprite;
        public static GameObject MacroseismographPrefab;
        public static GameObject MacroseismographConsumedPrefab;
        public static GameObject PotOfRegolithPrefab;
        public static GameObject ObsolutePrefab;

        // Buffs
        public static Sprite BirthdaySprite;
        public static Sprite CrysotopeFlySprite;
        public static Sprite DownloadedSprite;
        public static Sprite DreamSpeedSprite;
        public static Sprite EyeAwakeSprite;
        public static Sprite EyeDrowsySprite;
        public static Sprite EyeSleepySprite;
        public static Sprite FrisbeeGlideSprite;
        public static Sprite GrapeSprite;
        public static Sprite OverclockedSprite;
        //public static Sprite PeelSprite;
        public static Sprite QuarkGravitySprite;
        public static Sprite ResinBounceSprite;
        public static Sprite RottingSprite;
        public static Sprite SporkBleedSprite;
        public static Sprite SuesTeethSprite;
        public static Sprite TaffyChewSprite;
        public static Sprite TickCritSprite;
        public static Sprite ZorseStarveSprite;

        public static void Init()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Ultitems.PInfo.Location), "ultitembundle");
            //string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(Ultitems.PInfo.Location)), "assetbundle", "ultitembundle");
            Log.Debug("Path of bundle: " + path);
            mainBundle = AssetBundle.LoadFromFile(path);
            // For Local Testing
            if (mainBundle == null)
            {
                Log.Warning("Null Bundle... getting for Debug");
                path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(Ultitems.PInfo.Location)), "assetbundle", "ultitembundle");
                mainBundle = AssetBundle.LoadFromFile(path);
            }

            //float localScale = 0.1f;

            // * * * White * * * 
            CremeBruleeSprite = mainBundle.LoadAsset<Sprite>("CremeBrulee.png");
            FleaBagSprite = mainBundle.LoadAsset<Sprite>("FleaBag.png");
            FrisbeeSprite = mainBundle.LoadAsset<Sprite>("Frisbee.png");
            KoalaStickerSprite = mainBundle.LoadAsset<Sprite>("KoalaSticker.png");
            ToyRobotSprite = mainBundle.LoadAsset<Sprite>("ToyRobot.png");
            CremeBruleePrefab = mainBundle.LoadAsset<GameObject>("CremeBrulee.prefab");
            FleaBagPrefab = mainBundle.LoadAsset<GameObject>("FleaBag.prefab");
            FrisbeePrefab = mainBundle.LoadAsset<GameObject>("Frisbee.prefab");
            //FrisbeePrefab = mainBundle.LoadAsset<GameObject>("Frisbee3D.prefab");
            KoalaStickerPrefab = mainBundle.LoadAsset<GameObject>("KoalaSticker.prefab");
            ToyRobotPrefab = mainBundle.LoadAsset<GameObject>("ToyRobot.prefab");

            //CremeBruleePrefab.transform.localScale = Vector3.up * localScale;
            //FleaBagPrefab.transform.localScale = Vector3.up * localScale;
            //FrisbeePrefab.transform.localScale = Vector3.up * localScale;
            //KoalaStickerPrefab.transform.localScale = Vector3.up * localScale;
            //ToyRobotPrefab.transform.localScale = Vector3.up * localScale;

            // * * * Green * * * 
            BirthdayCandleSprite = mainBundle.LoadAsset<Sprite>("BirthdayCandles.png");
            DegreeScissorsSprite = mainBundle.LoadAsset<Sprite>("DegreeScissors.png");
            HMTSprite = mainBundle.LoadAsset<Sprite>("HMT.png");
            OverclockedGPUSprite = mainBundle.LoadAsset<Sprite>("OverclockedGPU.png");
            TinyIglooSprite = mainBundle.LoadAsset<Sprite>("TinyIgloo.png");
            //TippedArrowSprite = mainBundle.LoadAsset<Sprite>("TippedArrow.png");
            XenonAmpouleSprite = mainBundle.LoadAsset<Sprite>("XenonAmpoule.png");
            BirthdayCandlePrefab = mainBundle.LoadAsset<GameObject>("BirthdayCandle.prefab");
            DegreeScissorsPrefab = mainBundle.LoadAsset<GameObject>("DegreeScissors.prefab");
            HMTPrefab = mainBundle.LoadAsset<GameObject>("HMT.prefab");
            OverclockedGPUPrefab = mainBundle.LoadAsset<GameObject>("OverclockedGPU.prefab");
            TinyIglooPrefab = mainBundle.LoadAsset<GameObject>("TinyIgloo.prefab");
            //TippedArrowPrefab = mainBundle.LoadAsset<GameObject>("TippedArrow.prefab");
            XenonAmpoulePrefab = mainBundle.LoadAsset<GameObject>("XenonAmpoule.prefab");

            //BirthdayCandlePrefab.transform.localScale = Vector3.up * localScale;
            //DegreeScissorsPrefab.transform.localScale = Vector3.up * localScale;
            //HMTPrefab.transform.localScale = Vector3.up * localScale;
            //OverclockedGPUPrefab.transform.localScale = Vector3.up * localScale;
            //TippedArrowPrefab.transform.localScale = Vector3.up * localScale;
            //XenonAmpoulePrefab.transform.localScale = Vector3.up * localScale;

            // * * * Red * * * 
            CorrodingVaultSprite = mainBundle.LoadAsset<Sprite>("CorrodingVault.png");
            //FaultyBulbSprite = mainBundle.LoadAsset<Sprite>("FaultyBulb.png");
            GrapevineSprite = mainBundle.LoadAsset<Sprite>("Grapevine.png");
            PigsSporkSprite = mainBundle.LoadAsset<Sprite>("PigsSpork.png");
            RockyTaffySprite = mainBundle.LoadAsset<Sprite>("RockyTaffy.png");
            SuesMandiblesSprite = mainBundle.LoadAsset<Sprite>("SuesMandibles.png");
            ViralSmogSprite = mainBundle.LoadAsset<Sprite>("ViralSmog.png");
            CorrodingVaultPrefab = mainBundle.LoadAsset<GameObject>("CorrodingVault.prefab");
            //FaultyBulbPrefab = mainBundle.LoadAsset<GameObject>("FaultyBulb.prefab");
            GrapevinePrefab = mainBundle.LoadAsset<GameObject>("Grapevine.prefab");
            PigsSporkPrefab = mainBundle.LoadAsset<GameObject>("PigsSpork.prefab");
            RockyTaffyPrefab = mainBundle.LoadAsset<GameObject>("RockyTaffy.prefab");
            SuesMandiblesPrefab = mainBundle.LoadAsset<GameObject>("SuesMandibles.prefab");
            ViralSmogPrefab = mainBundle.LoadAsset<GameObject>("ViralSmog.prefab");

            //CorrodingVaultPrefab.transform.localScale = Vector3.up * localScale;
            //FaultyBulbPrefab.transform.localScale = Vector3.up * localScale;
            //GrapevinePrefab.transform.localScale = Vector3.up * localScale;
            //RockyTaffyPrefab.transform.localScale = Vector3.up * localScale;
            //SuesMandiblesPrefab.transform.localScale = Vector3.up * localScale;
            //ViralSmogPrefab.transform.localScale = Vector3.up * localScale;

            // * * * Void * * * 
            CrysotopeSprite = mainBundle.LoadAsset<Sprite>("Chrysotope.png");
            DownloadedRAMSprite = mainBundle.LoadAsset<Sprite>("DownloadedRAM.png");
            DriedHamSprite = mainBundle.LoadAsset<Sprite>("DriedHam.png");
            InhabitedCoffinSprite = mainBundle.LoadAsset<Sprite>("InhabitedCoffin.png");
            JubilantFoeSprite = mainBundle.LoadAsset<Sprite>("JubilantFoe.png");
            ResinWhirlpoolSprite = mainBundle.LoadAsset<Sprite>("ResinWhirlpool.png");
            RottenBonesSprite = mainBundle.LoadAsset<Sprite>("RottenBones.png");
            //TungstenRodSprite = mainBundle.LoadAsset<Sprite>("TungstenRod.png");
            //WormHolesSprite = mainBundle.LoadAsset<Sprite>("WormHoles.png");
            ZorsePillSprite = mainBundle.LoadAsset<Sprite>("ZorsePill.png");
            CrysotopePrefab = mainBundle.LoadAsset<GameObject>("Chrysotope.prefab");
            DownloadedRAMPrefab = mainBundle.LoadAsset<GameObject>("DownloadedRAM.prefab");
            DriedHamPrefab = mainBundle.LoadAsset<GameObject>("DriedHam.prefab");
            InhabitedCoffinPrefab = mainBundle.LoadAsset<GameObject>("InhabitedCoffin.prefab");
            JubilantFoePrefab = mainBundle.LoadAsset<GameObject>("JubilantFoe.prefab");
            ResinWhirlpoolPrefab = mainBundle.LoadAsset<GameObject>("ResinWhirlpool.prefab");
            RottenBonesPrefab = mainBundle.LoadAsset<GameObject>("RottenBones.prefab");
            //TungstenRodPrefab = mainBundle.LoadAsset<GameObject>("TungstenRod.prefab");
            //WormHolesPrefab = mainBundle.LoadAsset<GameObject>("WormHoles.prefab");
            ZorsePillPrefab = mainBundle.LoadAsset<GameObject>("ZorsePill.prefab");

            //CrysotopePrefab.transform.localScale = Vector3.up * localScale;
            //DownloadedRAMPrefab.transform.localScale = Vector3.up * localScale;
            //DriedHamPrefab.transform.localScale = Vector3.up * localScale;
            //InhabitedCoffinPrefab.transform.localScale = Vector3.up * localScale;
            //JubilantFoePrefab.transform.localScale = Vector3.up * localScale;
            //RottenBonesPrefab.transform.localScale = Vector3.up * localScale;
            //TungstenRodPrefab.transform.localScale = Vector3.up * localScale;
            //WormHolesPrefab.transform.localScale = Vector3.up * localScale;
            //ZorsePillPrefab.transform.localScale = Vector3.up * localScale;

            // * * * Lunar * * * 
            //CreatureDeckSprite = mainBundle.LoadAsset<Sprite>("CreatureDeck.png");
            DreamFuelSprite = mainBundle.LoadAsset<Sprite>("DreamFuel.png");
            UltravioletBulbSprite = mainBundle.LoadAsset<Sprite>("UltravioletBulb.png");
            //PowerChipsSprite = mainBundle.LoadAsset<Sprite>("PowerChips.png");
            SandPailSprite = mainBundle.LoadAsset<Sprite>("SandPail.png");
            SilverThreadSprite = mainBundle.LoadAsset<Sprite>("SilverThread.png");
            //CreatureDeckPrefab = mainBundle.LoadAsset<GameObject>("CreatureDeck.prefab");
            DreamFuelPrefab = mainBundle.LoadAsset<GameObject>("DreamFuel.prefab");
            UltravioletBulbPrefab = mainBundle.LoadAsset<GameObject>("UltravioletBulb.prefab");
            //PowerChipsPrefab = mainBundle.LoadAsset<GameObject>("PowerChips.prefab");
            SandPailPrefab = mainBundle.LoadAsset<GameObject>("SandPail.prefab");
            SilverThreadPrefab = mainBundle.LoadAsset<GameObject>("SilverThread.prefab");

            //CreatureDeckPrefab.transform.localScale = Vector3.up * localScale;
            //DreamFuelPrefab.transform.localScale = Vector3.up * localScale;
            //UltravioletBulbPrefab.transform.localScale = Vector3.up * localScale;
            //PowerChipsPrefab.transform.localScale = Vector3.up * localScale;
            //SandPailPrefab.transform.localScale = Vector3.up * localScale;
            //SilverThreadPrefab.transform.localScale = Vector3.up * localScale;

            // * * * Untiered * * * 
            CorrodingVaultConsumedSprite = mainBundle.LoadAsset<Sprite>("CorrodingVaultConsumed.png");
            InhabitedCoffinConsumedSprite = mainBundle.LoadAsset<Sprite>("InhabitedCoffinConsumed.png");
            SilverThreadConsumedSprite = mainBundle.LoadAsset<Sprite>("SilverThreadConsumed.png");
            SuesMandiblesConsumedSprite = mainBundle.LoadAsset<Sprite>("SuesMandiblesConsumed.png");
            UniversalSolventSprite = mainBundle.LoadAsset<Sprite>("UniversalSolvent.png");
            CorrodingVaultConsumedPrefab = mainBundle.LoadAsset<GameObject>("CorrodingVaultConsumed.prefab");
            InhabitedCoffinConsumedPrefab = mainBundle.LoadAsset<GameObject>("InhabitedCoffinConsumed.prefab");
            SilverThreadConsumedPrefab = mainBundle.LoadAsset<GameObject>("SilverThreadConsumed.prefab");
            SuesMandiblesConsumedPrefab = mainBundle.LoadAsset<GameObject>("SuesMandiblesConsumed.prefab");
            UniversalSolventPrefab = mainBundle.LoadAsset<GameObject>("UniversalSolvent.prefab");

            //CorrodingVaultConsumedPrefab.transform.localScale = Vector3.up * localScale;
            //InhabitedCoffinConsumedPrefab.transform.localScale = Vector3.up * localScale;
            //SuesMandiblesConsumedPrefab.transform.localScale = Vector3.up * localScale;
            //SilverThreadConsumedPrefab.transform.localScale = Vector3.up * localScale;
            //UniversalSolventPrefab.transform.localScale = Vector3.up * localScale;

            // * * * Equipment * * * 

            //JellyJailSprite = mainBundle.LoadAsset<Sprite>("JellyJail.png");
            IceCubesSprite = mainBundle.LoadAsset<Sprite>("IceCubes.png");
            //PetRockSprite = mainBundle.LoadAsset<Sprite>("PetRock.png");
            //TrebuchetSprite = mainBundle.LoadAsset<Sprite>("Trebuchet.png");
            OrbitalQuarkSprite = mainBundle.LoadAsset<Sprite>("OrbitalQuark.png");
            YieldSignSprite = mainBundle.LoadAsset<Sprite>("YieldSign.png");
            YieldSignStopSprite = mainBundle.LoadAsset<Sprite>("YieldSignStop.png");
            //JellyJailPrefab = mainBundle.LoadAsset<GameObject>("JellyJail.prefab");
            IceCubesPrefab = mainBundle.LoadAsset<GameObject>("IceCubes.prefab");
            //PetRockPrefab = mainBundle.LoadAsset<GameObject>("PetRock.prefab");
            //TrebuchetPrefab = mainBundle.LoadAsset<GameObject>("Trebuchet.prefab");
            OrbitalQuarkPrefab = mainBundle.LoadAsset<GameObject>("OrbitalQuark.prefab");
            YieldSignPrefab = mainBundle.LoadAsset<GameObject>("YieldSign.prefab");
            YieldSignStopPrefab = mainBundle.LoadAsset<GameObject>("YieldSignStop.prefab");

            //JellyJailPrefab.transform.localScale = Vector3.up * localScale;
            //IceCubesPrefab.transform.localScale = Vector3.up * localScale;
            //PetRockPrefab.transform.localScale = Vector3.up * localScale;
            //TrebuchetPrefab.transform.localScale = Vector3.up * localScale;

            // * * * Lunar Equipment * * *
            MacroseismographSprite = mainBundle.LoadAsset<Sprite>("Macroseismograph.png");
            MacroseismographConsumedSprite = mainBundle.LoadAsset<Sprite>("MacroseismographConsumed.png");
            PotOfRegolithSprite = mainBundle.LoadAsset<Sprite>("PotOfRegolith.png");
            ObsoluteSprite = mainBundle.LoadAsset<Sprite>("UniversalSolute.png");
            MacroseismographPrefab = mainBundle.LoadAsset<GameObject>("Macroseismograph.prefab");
            MacroseismographConsumedPrefab = mainBundle.LoadAsset<GameObject>("MacroseismographConsumed.prefab");
            PotOfRegolithPrefab = mainBundle.LoadAsset<GameObject>("PotOfRegolith.prefab");
            ObsolutePrefab = mainBundle.LoadAsset<GameObject>("UniversalSolute.prefab");

            //MacroseismographPrefab.transform.localScale = Vector3.up * localScale;
            //MacroseismographConsumedPrefab.transform.localScale = Vector3.up * localScale;
            //PotOfRegolithPrefab.transform.localScale = Vector3.up * localScale;
            //UniversalSolutePrefab.transform.localScale = Vector3.up * localScale;

            // * * * Buffs * * * 
            BirthdaySprite = mainBundle.LoadAsset<Sprite>("Birthday");
            CrysotopeFlySprite = mainBundle.LoadAsset<Sprite>("CrysotopeFly");
            DownloadedSprite = mainBundle.LoadAsset<Sprite>("Downloaded");
            DreamSpeedSprite = mainBundle.LoadAsset<Sprite>("DreamSpeed");
            EyeDrowsySprite = mainBundle.LoadAsset<Sprite>("EyeDrowsy");
            EyeAwakeSprite = mainBundle.LoadAsset<Sprite>("EyeAwake");
            EyeSleepySprite = mainBundle.LoadAsset<Sprite>("EyeSleepy");
            FrisbeeGlideSprite = mainBundle.LoadAsset<Sprite>("FrisbeeGlide");
            GrapeSprite = mainBundle.LoadAsset<Sprite>("Grape");
            OverclockedSprite = mainBundle.LoadAsset<Sprite>("Overclocked");
            //PeelSprite = mainBundle.LoadAsset<Sprite>("Peel");
            QuarkGravitySprite = mainBundle.LoadAsset<Sprite>("QuarkGravity");
            ResinBounceSprite = mainBundle.LoadAsset<Sprite>("ResinBounce");
            RottingSprite = mainBundle.LoadAsset<Sprite>("Rotting");
            SporkBleedSprite = mainBundle.LoadAsset<Sprite>("SporkBleed");
            SuesTeethSprite = mainBundle.LoadAsset<Sprite>("SuesTeeth");
            TaffyChewSprite = mainBundle.LoadAsset<Sprite>("TaffyChew");
            TickCritSprite = mainBundle.LoadAsset<Sprite>("TickCrit");
            ZorseStarveSprite = mainBundle.LoadAsset<Sprite>("ZorseStarve");

            /*/
            DegreeScissorsSprite = mainBundle.LoadAsset<Sprite>("DegreeScissors");
            OverclockedGPUSprite = mainBundle.LoadAsset<Sprite>("OverclockedGPU");
            FaultyBulbSprite = mainBundle.LoadAsset<Sprite>("FaultyBulb");
            ViralSmogSprite = mainBundle.LoadAsset<Sprite>("ViralSmog");
            DreamFuelSprite = mainBundle.LoadAsset<Sprite>("DreamFuel");
            CorrodingVaultSprite = mainBundle.LoadAsset<Sprite>("CorrodingVault");
            ToyRobotSprite = mainBundle.LoadAsset<Sprite>("ToyRobot");
            RustedVaultConsumedSprite = mainBundle.LoadAsset<Sprite>("CorrodingVaultConsumed");
            FleaBagSprite = mainBundle.LoadAsset<Sprite>("FleaBag");
            CremeBruleeSprite = mainBundle.LoadAsset<Sprite>("CremeBrulee");
            SuesMandiblesSprite = mainBundle.LoadAsset<Sprite>("SuesMandibles");
            SuesMandiblesConsumedSprite = mainBundle.LoadAsset<Sprite>("SuesMandiblesConsumed");

            IceCubesSprite = mainBundle.LoadAsset<Sprite>("IceCubes");
            PotOfRegolithSprite = mainBundle.LoadAsset<Sprite>("PotOfRegolith");

            DreamSpeedSprite = mainBundle.LoadAsset<Sprite>("DreamSpeed");
            OverclockedSprite = mainBundle.LoadAsset<Sprite>("Overclocked");
            BirthdaySprite = mainBundle.LoadAsset<Sprite>("Birthday");
            TickCritSprite = mainBundle.LoadAsset<Sprite>("TickCrit");
            //*/
        }
    }
}
