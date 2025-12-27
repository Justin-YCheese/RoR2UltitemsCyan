using BepInEx.Configuration;
using RoR2;

namespace UltitemsCyan.Items.Untiered
{

    // TODO: check if Item classes needs to be public
    public class CorrodingVaultConsumed : ItemBase
    {
        public static ItemDef item;

        public override void Init(ConfigFile configs)
        {
            item = CreateItemDef(
                "CORRODINGVAULTCONSUMED",
                "Corroding Vault (Corroded)",
                "It can't protect anything anymore...",
                "DESCRIPTION It can't protect anything anymore...",
                "Rusted Rusted Rusted",
                ItemTier.NoTier,
                UltAssets.CorrodingVaultConsumedSprite,
                UltAssets.CorrodingVaultConsumedPrefab,
                [ItemTag.Utility, ItemTag.OnStageBeginEffect, ItemTag.AIBlacklist],
                null,
                true
            );
        }

        protected override void Hooks() { }
    }
}