using R2API;
using RoR2;
using UltitemsCyan.Items.Tier1;

namespace UltitemsCyan.Buffs
{
    public class FrisbeeGlidingBuff : BuffBase
    {
        public static BuffDef buff;
        private const float airSpeed = Frisbee.airSpeed;

        public override void Init()
        {
            buff = DefineBuff("Frisbee Gliding Buff", false, false, UltAssets.FrisbeeGlideSprite);

            Hooks();
        }


        protected void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory && sender.HasBuff(buff))
            {
                int grabCount = sender.inventory.GetItemCountEffective(Frisbee.item);
                args.moveSpeedMultAdd += airSpeed / 100f * grabCount;
            }
        }
    }
}