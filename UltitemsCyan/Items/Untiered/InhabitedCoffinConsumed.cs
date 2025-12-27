using BepInEx.Configuration;
using RoR2;

namespace UltitemsCyan.Items.Untiered
{

    // TODO: check if Item classes needs to be public
    public class InhabitedCoffinConsumed : ItemBase
    {
        public static ItemDef item;

        public override void Init(ConfigFile configs)
        {
            item = CreateItemDef(
                "INHABITEDCOFFINCONSUMED",
                "Inhabited Coffin (Vaccant)",
                "It has been let loose...",
                "DESCRIPTION It has been let loose...",
                "Watch Out!",
                ItemTier.NoTier,
                UltAssets.InhabitedCoffinConsumedSprite,
                UltAssets.InhabitedCoffinConsumedPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility, ItemTag.OnStageBeginEffect, ItemTag.AIBlacklist],
                null,
                true
            );
        }

        protected override void Hooks() { }
    }
}