using BepInEx.Configuration;
using RoR2;

namespace UltitemsCyan.Items.Untiered
{

    // TODO: check if Item classes needs to be public
    public class SilverThreadConsumed : ItemBase
    {
        public static ItemDef item;

        public override void Init(ConfigFile configs)
        {
            item = CreateItemDef(
                "SILVERTHREADCONSUMED",
                "Silver Thread (Snapped)",
                "Proof of loss",
                "DESCRIPTION Proof of loss",
                "This is a garbage death zone. How did you get here?",
                ItemTier.NoTier,
                UltAssets.SilverThreadConsumedSprite,
                UltAssets.SilverThreadConsumedPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility, ItemTag.AIBlacklist]
            );
        }

        protected override void Hooks() { }
    }
}