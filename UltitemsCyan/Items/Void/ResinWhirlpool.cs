using RoR2;
using BepInEx.Configuration;
using UltitemsCyan.Items.Tier3;
using R2API;

namespace UltitemsCyan.Items.Void
{

    // TODO: check if Item classes needs to be public
    public class ResinWhirlpool : ItemBase
    {
        public static ItemDef item;
        public static ItemDef transformItem;

        public const float bounceJumpPowMult = 15f;
        public const int maxBouncePerItem = 8;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Resin Whirlpool";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            item = CreateItemDef(
                "RESINWHIRLPOOL",
                itemName,
                "Gain extra jumps and jump power on kill. <style=cIsVoid>Corrupts all Viral Smogs</style>.",
                "Gain <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> jumps and +20% jump power on kill until you land. Store up to <style=cIsUtility>10</style> <style=cStack>(+10 per stack)</style> extra jumps. <style=cIsVoid>Corrupts all Viral Smogs</style>.",
                "This Tape is so bouncy! And it just seems endless...\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nWait did you name it Quantum Peel because you wanted a banana item but you added too many food items?\nShhh! They'll never know as long as they never read the description...\n\nand also I changed the item so now it's more aboue bounces instead of speed...\n\nso yeah...",
                ItemTier.VoidTier3,
                UltAssets.ResinBounceSprite,
                UltAssets.ResinWhirlpoolPrefab,
                [ItemTag.Utility],
                ViralEssence.item
            );
        }

        protected override void Hooks()
        {
            // On kill gain stack
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            // On Jumping remove stack
            //On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
            // Touch ground reset buff
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            if (self && damageReport.attacker && damageReport.attackerBody && damageReport.attackerBody.inventory)
            {
                CharacterBody killer = damageReport.attackerBody;
                int grabCount = killer.inventory.GetItemCountEffective(item);
                int buffCount = killer.GetBuffCount(Buffs.ResinBounceBuff.buff);
                // If body has the item and has fewer than the max stack then add buff
                if (grabCount > 0 && buffCount < maxBouncePerItem * grabCount) // maxOverclockedPerStack * grabCount
                {
                    // Don't have any buffs yet
                    _ = Util.PlaySound("Play_item_proc_crowbar", killer.gameObject);
                    //_ = Util.PlaySound("Play_wDroneDeath", killer.gameObject);

                    killer.AddBuff(Buffs.ResinBounceBuff.buff);
                }
            }
            // TODO check if goes in beginning or end
            orig(self, damageReport);
        }

        //private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, EntityStates.GenericCharacterMain self)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}