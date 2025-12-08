using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;
using static UltitemsCyan.Equipment.YieldSign;

namespace UltitemsCyan.Equipment
{
    public class YieldSignStop : EquipmentBase
    {
        public static EquipmentDef equipment;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Yield Sign";
            if (!CheckItemEnabledConfig(itemName, "Equipment", configs))
            {
                return;
            }
            equipment = CreateItemDef(
                "YIELDSIGNSTOP",
                itemName,
                "Alternate between multiplying speed and canceling it. Hit nearby enemies each time.",
                "Alternate between multipling speed by 400%, or canceling speed. Damage nearby enemies for 300% damage.",
                "Just Stop",
                cooldown,
                false,
                false,
                false,
                UltAssets.YieldSignStopSprite,
                UltAssets.YieldSignStopPrefab
            );
        }

        protected override void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            On.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += EquipmentSlot_RpcOnClientEquipmentActivationRecieved;
        }

        private void EquipmentSlot_RpcOnClientEquipmentActivationRecieved(On.RoR2.EquipmentSlot.orig_RpcOnClientEquipmentActivationRecieved orig, EquipmentSlot self)
        {
            orig(self);
            if (self.equipmentIndex == equipment.equipmentIndex && self.characterBody && self.characterBody.characterMotor)
            {
                Log.Debug("RPC Equipment | Net? " + NetworkServer.active);
                if (NetworkServer.active)
                {
                    // Boost Multipliers because item switches on server first
                    YieldBoostActivation(self);
                }
                else
                {
                    YieldStopActivation(self);
                }
            }
        }

        //
        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == equipment)
            {
                Log.Debug("Yields qStop");
                self.characterBody.inventory.SetEquipmentIndex(YieldSign.equipment.equipmentIndex, true);
                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
        //*/
    }
}