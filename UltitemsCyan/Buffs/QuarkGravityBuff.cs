using R2API;
using RoR2;
using System;
using UltitemsCyan.Equipment;
using UltitemsCyan.Items;
using UltitemsCyan.Items.Tier1;
using UnityEngine.Networking;
using static RoR2.DotController;
using static UnityEngine.Rendering.DebugUI;

namespace UltitemsCyan.Buffs
{
    public class QuarkGravityBuff : BuffBase
    {
        public static BuffDef buff;
        public static DotIndex index;

        //private const float airSpeed = Chrysotope.airSpeed;
        private static readonly float speed = OrbitalQuark.buffSpeed;
        private static readonly float dampening = OrbitalQuark.buffDampening;
        private static readonly float fallSpeed = OrbitalQuark.buffFallSpeed;
        private static readonly float minFallSpeed = OrbitalQuark.buffMinFallSpeed;

        public override void Init()
        {
            buff = DefineBuff("Quark Gravity Buff", false, false, UltAssets.QuarkGravitySprite, false, false, true);
            //Log.Info(buff.name + " Initialized");

            Hooks();
        }

        protected void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            if (self && buffDef == buff)
            {
                //Log.Debug("Got Gravity");
                CharacterMotor chMotor = self.characterMotor;
                if (chMotor)
                {
                    CharacterGravityParameters gravPrarameters = chMotor.gravityParameters;
                    gravPrarameters.channeledAntiGravityGranterCount++;
                    chMotor.gravityParameters = gravPrarameters;
                    //chMotor.velocity *= dampening;
                    self.statsDirty = true;

                    _ = self.AddItemBehavior<QuarkGravityBehavior>(1);
                }
            }
            orig(self, buffDef);
        }

        private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if (self && buffDef == buff)
            {
                CharacterMotor chMotor = self.characterMotor;
                if (chMotor)
                {
                    CharacterGravityParameters gravPrarameters = chMotor.gravityParameters;
                    gravPrarameters.channeledAntiGravityGranterCount--;
                    chMotor.gravityParameters = gravPrarameters;
                    self.statsDirty = true;

                    _ = self.AddItemBehavior<QuarkGravityBehavior>(0);
                }
            }
            orig(self, buffDef);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory && sender.teamComponent.teamIndex == TeamIndex.Player && sender.HasBuff(buff))
            {
                args.moveSpeedMultAdd += speed / 100f;
                //attackSpeedPerStack / 100f * buffCount;
                //sender.characterMotor.velocity *= .8;
                //sender.maxJumpCount += jumpCount;
                /*Log.Debug(" v v v v v New Quark Stats:" +
                    "\n\njump\t" + sender.maxJumpCount +
                    "\nGrNeu\t" + sender.characterMotor._gravityParameters.antiGravityNeutralizerCount +
                    "\nusGra\t" + sender.characterMotor.useGravity // true | false when in flying
                    );*/
            }
        }

        public class QuarkGravityBehavior : CharacterBody.ItemBehavior
        {
            private CharacterMotor motor;
            private bool _hasQuarkBuff = false;

            public bool QuarkBuff
            {
                get { return _hasQuarkBuff; }
                set
                {
                    // If not already the same value
                    if (_hasQuarkBuff != value)
                    {
                        //Log.Warning(body.name + " " + value + " gravity toggled!: " + _hasQuarkBuff);

                        _hasQuarkBuff = value;
                    }
                }
            }

            public void FixedUpdate()
            {
                if (motor)
                {
                    QuarkBuff = body.HasBuff(buff);
                    if (QuarkBuff)
                    {
                        //Log.Warning(body.name + " >>*>>*>> Is on server? " + NetworkServer.active + " old Grave motor (" + motor.velocity.x + " ," + motor.velocity.y + " ," + motor.velocity.z + ")");
                        motor.velocity.x *= dampening;
                        motor.velocity.z *= dampening;

                        // Player is stalling
                        if (Math.Abs(motor.velocity.y) < fallSpeed)
                        {
                            
                            motor.velocity.y = (motor.velocity.y + fallSpeed) * dampening - fallSpeed;
                            // Floating point inaccuracy requires something to pass -.01 fall speed
                            if (Math.Abs(motor.velocity.y) < minFallSpeed)
                            {
                                motor.velocity.y -= minFallSpeed / 5f;
                            }
                        }
                        else
                        {
                            motor.velocity.y *= dampening;
                        }
                        //Log.Warning(body.name + " >>*>>*>> NEW Grave motor (" + motor.velocity.x + " ," + motor.velocity.y + " ," + motor.velocity.z + ")");
                    }
                }
            }

            public void Start()
            {
                motor = body.characterMotor;
                //enabled = false;
            }

#pragma warning disable IDE0051 // Remove unused private members
            private void OnDisable()
#pragma warning restore IDE0051 // Remove unused private members
            {
                QuarkBuff = false;
            }

            public void OnDestroy()
            {
                QuarkBuff = false;
            }
        }
    }
}