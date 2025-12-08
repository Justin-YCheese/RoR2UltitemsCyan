using R2API;
using RoR2;
using UltitemsCyan.Items.Lunar;

namespace UltitemsCyan.Buffs
{
    public class DreamSpeedBuff : BuffBase
    {
        public static BuffDef buff;
        private const float dreamSpeed = DreamFuel.dreamSpeed;

        public override void Init()
        {
            buff = DefineBuff("Dream Fuel buff", false, false, UltAssets.DreamSpeedSprite);
            //Log.Info(buff.name + " Initialized");

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
                int grabCount = sender.inventory.GetItemCountEffective(DreamFuel.item);
                args.moveSpeedMultAdd += dreamSpeed / 100f * grabCount;
            }
        }
    }
}