using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine.Networking;

namespace UltitemsCyan.Equipment
{
    // TODO: check if Item classes needs to be public
    public class MacroseismographConsumed : EquipmentBase
    {
        public static EquipmentDef equipment;

        private const float cooldown = 300f;

        public override void Init(ConfigFile configs)
        {
            if (!CheckItemEnabledConfig("Macroseismograph", "Equipment", configs))
            {
                return;
            }
            equipment = CreateItemDef(
                "MACROSEISMOGRAPHCONSUMED",
                "Macro Friend",
                "It's broken something inside you...   but it'll never leave you",
                "And now it will never leave you",
                "Broken soul, and forever buddy",
                cooldown,
                true,
                false,
                false,
                UltAssets.MacroseismographConsumedSprite,
                UltAssets.MacroseismographConsumedPrefab
            );
            LanguageAPI.Add("EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_ACTION", "together...  forever...");
            LanguageAPI.Add("EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_ONE", "i...  won't...  let...  you...");
            LanguageAPI.Add("EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_TWO", "we...  are...  friends...");
            LanguageAPI.Add("EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_THREE", "I'm...  always...  here...  for...  you...");
            LanguageAPI.Add("EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_FOUR", "forever...  is...  a...  long...  time...");
        }

        protected override void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            // Prevent player from picking up other equipments
            //On.RoR2.Inventory.GiveEquipmentString += Inventory_GiveEquipmentString;
            On.RoR2.EquipmentDef.AttemptGrant += EquipmentDef_AttemptGrant;
        }

        private void EquipmentDef_AttemptGrant(On.RoR2.EquipmentDef.orig_AttemptGrant orig, ref PickupDef.GrantContext context)
        {
            //Log.Warning(" * * * Macroseismograph is in the house!");
            // If player has Macroseismograph as their current item
            if (context.body && context.body.inventory.currentEquipmentIndex == equipment.equipmentIndex)
            {

                string token = "EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_ONE";
                Xoroshiro128Plus rng = new(Run.instance.stageRng.nextUlong);
                switch (rng.RangeInt(1, 4))
                {
                    case 0:
                        break;
                    case 1:
                        token = "EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_TWO";
                        break;
                    case 2:
                        token = "EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_THREE";
                        break;
                    case 3:
                        token = "EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_GRANT_FOUR";
                        break;
                    default:
                        break;
                }
                Chat.SendBroadcastChat(new Chat.BodyChatMessage
                {
                    bodyObject = context.body.gameObject,
                    token = token
                });
                _ = Util.PlaySound("Play_blindVermin_idle_VO", context.body.gameObject);
                return;
            }
            else
            {
                orig(ref context);
                //Log.Warning(" * * * Macroseismograph has left the building...");
            }
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active && self.equipmentDisabled && equipmentDef == equipment)
            {
                //Log.Debug("together forever...");

                Chat.SendBroadcastChat(new Chat.BodyChatMessage
                {
                    bodyObject = self.characterBody.gameObject,
                    token = "EQUIPMENT_MACROSEISMOGRAPHCONSUMED_CHAT_ACTION"
                });

                self.subcooldownTimer = 2f;
                _ = Util.PlaySound("Play_blindVermin_death", self.characterBody.gameObject);
                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
    }
}