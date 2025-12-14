using BepInEx.Configuration;
using RoR2;

namespace UltitemsCyan.Equipment
{
    // TODO: check if Item classes needs to be public
    public class PotOfRegolith : EquipmentBase
    {
        public static EquipmentDef equipment;

        private const float cooldown = 2f;
        private const float subCooldown = .2f;

        private const float baseDamageChance = 75f;
        private const float basePercentDamage = 4f;
        private const float maxPercentDamage = 20f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Pot of Regolith";
            if (!CheckItemEnabledConfig(itemName, "Equipment", configs))
            {
                return;
            }
            equipment = CreateItemDef(
                "POTOFREGOLITH",
                itemName,
                "<style=cDeath>Take damage</style> on use.",
                "Take <style=cIsHealth>2% or 20% of your health</style> as <style=cIsDamage>damage</style>. Only deals <style=cIsHealth>2%</style> while at low health.",
                "The dust is as sharp as a knife",
                cooldown,
                true,
                true,
                false,
                UltAssets.PotOfRegolithSprite,
                UltAssets.PotOfRegolithPrefab
            );
        }

        protected override void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == equipment)
            {
                CharacterBody activator = self.characterBody;
                //UnityEngine.Random.Range(basePercentDamage, maxPercentDamage);

                float percentDamage = maxPercentDamage;
                Log.Debug("Pot isHealthLow?" + activator.healthComponent.isHealthLow);
                if (activator.healthComponent.isHealthLow || Util.CheckRoll(baseDamageChance, activator.master.luck))
                {
                    percentDamage = basePercentDamage;
                }
                else
                {
                    //Log.Debug("Pot High Damage");
                }

                DamageInfo damageSelf = new()
                {
                    crit = false, // activator.RollCrit()
                    damage = percentDamage / 100f * activator.healthComponent.fullCombinedHealth, // + activator.baseDamage
                    procCoefficient = 100f,
                    damageType = DamageType.Generic,
                    inflictor = activator.gameObject,
                    position = activator.transform.position
                };
                //if (damageSelf.crit) { damageSelf.damage *= 2; }

                //Log.Debug("Pot activator damage: " + damageSelf.damage);

                activator.healthComponent.TakeDamage(damageSelf);
                //self.subcooldownTimer += 5f;
                //Log.Debug("Sub cooldown");
                _ = Util.PlaySound("Play_imp_attack", activator.gameObject);
                self.subcooldownTimer = subCooldown;

                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
    }
}