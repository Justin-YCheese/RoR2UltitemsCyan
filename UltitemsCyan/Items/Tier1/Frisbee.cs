using BepInEx.Configuration;
using RoR2;
using UltitemsCyan.Buffs;
//using static RoR2.GenericPickupController;

namespace UltitemsCyan.Items.Tier1
{

    // TODO: check if Item classes needs to be public
    public class Frisbee : ItemBase
    {
        public static ItemDef item;

        public const float airSpeed = 10f;

        public const float dampeningForce = 0.4f;
        public const float riseSpeed = 4f;
        // Constant rise of 2.0667

        public const float baseDuration = 1.2f;
        public const float durationPerStack = 0.6f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Frisbee";
            if (!CheckItemEnabledConfig(itemName, "White", configs))
            {
                return;
            }
            item = CreateItemDef(
                "FRISBEE",
                itemName,
                "Rise and move faster after jumping. Hold jump to continue gliding.",
                "Rise slowly and move <style=cIsUtility>10%</style> <style=cStack>(+10% per stack)</style> faster after jumping for <style=cIsUtility>1.2</style> <style=cStack>(+0.6 per stack)</style> seconds. Hold jump to keep hovering.",
                "Folding Flyers Falling Futher Faster For Five Far Fields",
                ItemTier.Tier1,
                UltAssets.FrisbeeSprite,
                UltAssets.FrisbeePrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.inventory)
            {
                _ = self.AddItemBehavior<FrisbeeBehavior>(self.inventory.GetItemCountEffective(item));
            }
        }

        // * * * Why was this so hard...
        // GenericCharacterMain_ProcessJump Runs only on client side, but can't run buff functions on client
        // Can't use AddBuffTimedAuthority because no server function to properly remove a timed function
        // Must use SetBuffCount which works on client side
        // Must use my own function to time the buff

        private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, EntityStates.GenericCharacterMain self)
        {
            
            if (self.characterBody && self.characterBody.inventory)
            {
                CharacterBody body = self.characterBody;
                int grabCount = body.inventory.GetItemCountEffective(item);

                if (grabCount > 0 && self.hasCharacterMotor && self.jumpInputReceived)
                {
                    //Log.Warning("Is Authority? " + self.isAuthority + " Is Local? " + self.isLocalPlayer + " Is Local Authority? " + self.localPlayerAuthority + " is? " + self.rigidbody);
                    // For both host and not host
                    // True | False | True | rigidbody

                    //Log.Debug("characterMotor, jumpInput, body: " + self.hasCharacterMotor + " | " + self.jumpInputReceived + " | " + self.characterBody);
                    //Log.Debug("Jumps: " + self.characterMotor.jumpCount + " < " + self.characterBody.maxJumpCount);
                    if (self.characterMotor.jumpCount < self.characterBody.maxJumpCount)
                    {
                        //   *   *   *   ADD EFFECT   *   *   *   //

                        //Log.Debug("Frisbee Jump ? ? ? adding buff for " + (baseDuration + durationPerStack * (grabCount - 1)) + " seconds");
                        //self.characterBody.AddTimedBuffAuthority(FrisbeeFlyingBuff.buff.buffIndex, baseDuration + (durationPerStack * (grabCount - 1)));

                        FrisbeeBehavior behavior = self.characterBody.GetComponent<FrisbeeBehavior>();
                        behavior.enabled = true;
                        behavior.UpdateStopwatch(Run.instance.time);
                        body.SetBuffCount(FrisbeeGlidingBuff.buff.buffIndex, 1); // TODO Change to add buff?

                        //Log.Debug("Has Timed def Buff? " + self.HasBuff(FrisbeeGlidingBuff.buff));
                    }
                }
            }
            orig(self);
        }




        public class FrisbeeBehavior : CharacterBody.ItemBehavior
        {
            private CharacterMotor motor;
            private const float baseDuration = Frisbee.baseDuration;
            private const float durationPerStack = Frisbee.durationPerStack;
            public float flyingStopwatch = 0;
            private bool _canHaveBuff = false;
            //private bool _jumpBuffer = false; //TODO add this functionality

            public void UpdateStopwatch(float newTime)
            {
                //Log.Debug("New attack at " + newTime);
                flyingStopwatch = newTime;
            }

            public bool CanHaveBuff
            {
                get { return _canHaveBuff; }
                set
                {
                    // If not already the same value
                    if (_canHaveBuff != value)
                    {
                        //Log.Debug("Frisbee!!! grounded? " + !motor.isGrounded + " jumping? " + body.inputBank.jump.down);

                        /*/
                        if (value)
                        {
                            _canHaveBuff = true;
                            _jumpBuffer = true;
                        }
                        else if (_jumpBuffer)
                        {
                            _jumpBuffer = false;
                        }
                        else
                        {
                            _canHaveBuff = false;
                        }
                        //*/

                        _canHaveBuff = value;

                        if (!_canHaveBuff)
                        {
                            //Log.Warning("Frisbee!!! Can't have buff, removing buff");
                            body.SetBuffCount(FrisbeeGlidingBuff.buff.buffIndex, 0);
                        }
                    }
                }
            }

            public void FixedUpdate()
            {
                if (motor)
                {
                    CanHaveBuff = !motor.isGrounded && body.inputBank.jump.down
                        && Run.instance.time <= flyingStopwatch + baseDuration + durationPerStack * (stack - 1);
                    if (body.HasBuff(FrisbeeGlidingBuff.buff))
                    {
                        // Player is rising
                        if (motor.velocity.y < riseSpeed)
                        {
                            //Log.Warning("Is on server? " + NetworkServer.active + "  Or enabled? " + motor.enabled + "last velocity: " + motor.lastVelocity);
                            //Log.Debug("Falling?: \t" + motor.velocity.y + " = " + ((body.characterMotor.velocity.y * dampeningForce) + (riseSpeed * dampeningForce)));
                            //body.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * fallReducedGravity;
                            motor.velocity.y = (motor.velocity.y - riseSpeed) * dampeningForce + riseSpeed;
                        }
                    }
                }
            }

            public void Start()
            {
                motor = body.characterMotor;
                enabled = false;
            }

#pragma warning disable IDE0051 // Remove unused private members
            private void OnDisable()
#pragma warning restore IDE0051 // Remove unused private members
            {
                flyingStopwatch = 0;
                CanHaveBuff = false;
            }

            public void OnDestroy()
            {
                flyingStopwatch = 0;
                CanHaveBuff = false;
            }
        }
    }
}