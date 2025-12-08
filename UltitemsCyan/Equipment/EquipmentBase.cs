using R2API;
using RoR2;
using UltitemsCyan.Items;
using UnityEngine;

namespace UltitemsCyan.Equipment
{

    // TODO: check if Item classes needs to be public
    public abstract class EquipmentBase : ItemBase
    {
        //public static EquipmentDef equipment;

        public EquipmentDef CreateItemDef(
            string tokenPrefix,
            string name,
            string pick,
            string desc,
            string lore,
            float cooldown,
            bool isLunar,
            bool canDrop,
            bool randomlyTriggered,
            //bool enigmaCompatible,
            Sprite sprite,
            GameObject prefab
        )
        {
            EquipmentDef equipment = ScriptableObject.CreateInstance<EquipmentDef>();

            LanguageAPI.Add(tokenPrefix + "_NAME", name);
            LanguageAPI.Add(tokenPrefix + "_PICK", pick);
            LanguageAPI.Add(tokenPrefix + "_DESC", desc);
            LanguageAPI.Add(tokenPrefix + "_LORE", lore);

            equipment.name = tokenPrefix + "_NAME";
            equipment.nameToken = tokenPrefix + "_NAME";
            equipment.pickupToken = tokenPrefix + "_PICK";
            equipment.descriptionToken = tokenPrefix + "_DESC";
            equipment.loreToken = tokenPrefix + "_LORE";

            //Log.Debug("Init " + equipment.name);

            equipment.cooldown = cooldown;

            equipment.isLunar = isLunar;
            if (isLunar)
            {
                equipment.colorIndex = ColorCatalog.ColorIndex.LunarItem;
                equipment.canBeRandomlyTriggered = false;
            }

            equipment.canBeRandomlyTriggered = randomlyTriggered; // By default is true
            equipment.enigmaCompatible = true;

            equipment.pickupIconSprite = sprite;
#pragma warning disable CS0618 // Type or member is obsolete
            equipment.pickupModelPrefab = prefab; // TODO Change to use Reference
#pragma warning restore CS0618 // Type or member is obsolete

            equipment.appearsInSinglePlayer = true;
            equipment.appearsInMultiPlayer = true;

            equipment.canDrop = canDrop;
            equipment.isBoss = false;

            ItemDisplayRuleDict displayRules = new(null);
            _ = ItemAPI.Add(new CustomEquipment(equipment, displayRules));

            // Item Functionality
            Hooks();

            GetEquipmentDef = equipment;

            return equipment;
        }
        //public abstract void Init();
    }
}