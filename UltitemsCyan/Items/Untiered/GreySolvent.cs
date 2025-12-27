using BepInEx.Configuration;
using RoR2;

namespace UltitemsCyan.Items.Untiered
{

    // TODO: check if Item classes needs to be public
    public class GreySolvent : ItemBase
    {
        public static ItemDef item;

        public override void Init(ConfigFile configs)
        {
            item = CreateItemDef(
                "GREYSOLVENT",
                "Grey Solvent",
                "Everything returns...",
                "DESCRIPTION Everything returns...",
                "So a Universal Solute just turns other things into Universal Solvents?\n" +
                "I guess that makes sense... becasue if there is a universal solute, then everything else desolves it.\n" +
                "So then everything else is a universal solvent for the universal solute",
                ItemTier.NoTier,
                UltAssets.UniversalSolventSprite,
                UltAssets.UniversalSolventPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility, ItemTag.AIBlacklist],
                null,
                true
            );
        }

        protected override void Hooks() { }
    }
}