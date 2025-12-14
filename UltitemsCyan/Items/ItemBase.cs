using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System.Linq;
using UnityEngine;

namespace UltitemsCyan.Items
{

    // TODO: check if Item classes needs to be public
    public abstract class ItemBase
    {
        private readonly ExpansionDef Sovt = ExpansionCatalog.expansionDefs.FirstOrDefault(expansion => expansion.nameToken == "DLC1_NAME");
        public abstract void Init(ConfigFile configs);
        public ItemDef CreateItemDef(
            string tokenPrefix,
            string name,
            string pick,
            string desc,
            string lore,
            ItemTier tier,
            Sprite sprite,
            GameObject prefab,
            ItemTag[] tags,
            ItemDef transformItem = null)
            // TODO Add IsConsumed
        {
            ItemDef item = ScriptableObject.CreateInstance<ItemDef>();

            LanguageAPI.Add(tokenPrefix + "_NAME", name);
            LanguageAPI.Add(tokenPrefix + "_PICK", pick);
            LanguageAPI.Add(tokenPrefix + "_DESC", desc);
            LanguageAPI.Add(tokenPrefix + "_LORE", lore);

            item.name = tokenPrefix + "_NAME";
            item.nameToken = tokenPrefix + "_NAME";
            item.pickupToken = tokenPrefix + "_PICK";
            item.descriptionToken = tokenPrefix + "_DESC";
            item.loreToken = tokenPrefix + "_LORE";
            //item.requiredExpansion = ExpansionDef.;
            //item.isConsumed

            // tier
            ItemTierDef itd = ScriptableObject.CreateInstance<ItemTierDef>();
            itd.tier = tier;
            item._itemTierDef = itd;

            item.canRemove = tier != ItemTier.NoTier;
            item.hidden = false;

            item.pickupIconSprite = sprite;
#pragma warning disable CS0618 // Type or member is obsolete
            item.pickupModelPrefab = prefab;
#pragma warning restore CS0618 // Type or member is obsolete

            item.tags = tags;

            ItemDisplayRuleDict displayRules = new(null);

            _ = ItemAPI.Add(new CustomItem(item, displayRules));

            // Item Functionality
            Hooks();

            //Log.Info("Test Item Initialized");
            GetItemDef = item;
            if (transformItem)
            {
                //Log.Warning("Transform from + " + transformItem.name);
                GetTransformItem = transformItem;
                item.requiredExpansion = Sovt;
            }
            //Log.Warning(" Initialized: " + item.name);
            return item;
        }

        public bool CheckItemEnabledConfig(string name, string tier, ConfigFile configs)
        {
            return configs.Bind(
                "Enable " + tier + " Items",
                "Enable " + name + "?",
                true
            ).Value;
        }

        protected abstract void Hooks();

        public ItemDef GetItemDef { get; set; }
        public EquipmentDef GetEquipmentDef { get; set; }
        public ItemDef GetTransformItem { get; set; }
    }
}