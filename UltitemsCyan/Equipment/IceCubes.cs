using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;

namespace UltitemsCyan.Equipment
{

    // TODO: check if Item classes needs to be public
    public class IceCubes : EquipmentBase
    {
        // Inflict Slowdown on self?
        public static EquipmentDef equipment;
        private const float percentOfBarrier = 50f;
        private const float flatBarrier = 100f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "9 Ice Cubes";
            if (!CheckItemEnabledConfig(itemName, "Equipment", configs))
            {
                return;
            }
            equipment = CreateItemDef(
                "ICECUBES",
                itemName,
                "Gain barrier on use",
                "Instantly gain <style=cIsHealing>temporary barrier</style> for <style=cIsHealing>100 health</style> plus an additional <style=cIsHealing>50%</style> of <style=cIsHealing>maximum health</style>",
                "Alice that freezes forever",
                60f,
                false,
                true,
                true,
                UltAssets.IceCubesSprite,
                UltAssets.IceCubesPrefab
            );
        }

        protected override void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active && self.equipmentDisabled && equipmentDef == equipment)
            {
                CharacterBody activator = self.characterBody;
                activator.healthComponent.AddBarrier(activator.healthComponent.fullBarrier * percentOfBarrier / 100f + flatBarrier);
                //Log.Debug("Ice Gained: " + activator.healthComponent.fullBarrier * percentOfBarrier / 100f + flatBarrier);
                _ = Util.PlaySound("Play_item_proc_iceRingSpear", self.gameObject);
                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
    }
}