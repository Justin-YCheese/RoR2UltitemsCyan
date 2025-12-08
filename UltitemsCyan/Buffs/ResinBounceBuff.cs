using R2API;
using RoR2;
using UltitemsCyan.Items.Void;

namespace UltitemsCyan.Buffs
{
    public class ResinBounceBuff : BuffBase
    {
        public static BuffDef buff;

        private const float jumpPowMult = ResinWhirlpool.bounceJumpPowMult;

        public override void Init()
        {
            buff = DefineBuff("Resin Bounce Buff", true, false, UltAssets.ResinBounceSprite);
            Log.Info(buff.name + " Initialized");

            Hooks();
        }

        protected void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory && sender.HasBuff(buff) && sender.characterMotor)
            {
                int buffCount = sender.GetBuffCount(buff);
                // Calculate Logistic Growth of Speed Po = 20
                //double totalSpeed = Math.E;
                //totalSpeed = 1 + (pLogiConstant * Math.Pow(totalSpeed, buffCount * -pLogiRate));
                //totalSpeed = pLogiLimit / totalSpeed * grabCount / 100;
                Log.Debug(" s s s @ s s s | Bouncing at " + jumpPowMult / 100 * buffCount);
                args.jumpPowerMultAdd += jumpPowMult / 100 * buffCount;
            }
        }
    }
}