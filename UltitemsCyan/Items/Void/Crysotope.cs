using RoR2;
using UltitemsCyan.Buffs;
using UltitemsCyan.Items.Tier1;
using BepInEx.Configuration;

namespace UltitemsCyan.Items.Void
{

    // TODO: check if Item classes needs to be public
    public class Crysotope : ItemBase
    {
        public static ItemDef item;
        public static ItemDef transformItem;

        //public const float airSpeed = 10f;

        public const float dampeningForce = 0.9f;
        public const float riseSpeed = 18f;
        // Constant rise of 13.5 speed?

        public const float baseDuration = 0.6f;
        public const float durationPerStack = 0.4f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Crysotope";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            item = CreateItemDef(
                "CRYSOTOPE",
                itemName,
                "Rise after jumping. Hold jump to continue flying. <style=cIsVoid>Corrupts all Frisbees</style>.",
                "Rise after jumping for <style=cIsUtility>0.6</style> <style=cStack>(+0.4 per stack)</style> seconds. Hold jump to keep flying. <style=cIsVoid>Corrupts all Frisbees</style>.",
                "An eyeless crystal snake capbable of flying 100 meters",
                ItemTier.VoidTier1,
                UltAssets.CrysotopeSprite,
                UltAssets.CrysotopePrefab,
                [ItemTag.Utility],
                Frisbee.item
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
                _ = self.AddItemBehavior<CrysotopeBehavior>(self.inventory.GetItemCountEffective(item));
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

                        Log.Debug("Crysotope Jump ? ? ? adding buff for " + (baseDuration + durationPerStack * (grabCount - 1)) + " seconds");
                        //self.characterBody.AddTimedBuffAuthority(FrisbeeFlyingBuff.buff.buffIndex, baseDuration + (durationPerStack * (grabCount - 1)));

                        CrysotopeBehavior behavior = self.characterBody.GetComponent<CrysotopeBehavior>();
                        behavior.enabled = true;
                        behavior.UpdateStopwatch(Run.instance.time);
                        body.SetBuffCount(CrysotopeFlyingBuff.buff.buffIndex, 1);

                        Log.Debug("Has Timed def Buff? " + self.HasBuff(CrysotopeFlyingBuff.buff));
                    }
                }
            }
            orig(self);
        }




        public class CrysotopeBehavior : CharacterBody.ItemBehavior
        {
            private CharacterMotor characterMotor;
            private const float baseDuration = Crysotope.baseDuration;
            private const float durationPerStack = Crysotope.durationPerStack;
            public float flyingStopwatch = 0;
            private bool _canHaveBuff = false;

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
                        _canHaveBuff = value;
                        if (!_canHaveBuff)
                        {
                            //Log.Debug("y = " + body.characterMotor.velocity.y + " | x = " + body.characterMotor.velocity.x);
                            //body.characterMotor.velocity.y = Math.Min(body.characterMotor.velocity.y, 2f);
                            //body.characterMotor.velocity *= 2f;
                            //body.characterMotor.velocity.x *= 1.2f;
                            //Log.Debug(" + new y = " + body.characterMotor.velocity.y + " | x = " + body.characterMotor.velocity.x + " | z = " + body.characterMotor.velocity.z);
                            body.SetBuffCount(CrysotopeFlyingBuff.buff.buffIndex, 0);
                        }
                    }
                }
            }

            // If player is at full health
            public void FixedUpdate()
            {
                if (characterMotor)
                {
                    CanHaveBuff = !characterMotor.isGrounded && body.inputBank.jump.down
                        && Run.instance.time <= flyingStopwatch + baseDuration + durationPerStack * (stack - 1);
                    if (body.HasBuff(CrysotopeFlyingBuff.buff))
                    {
                        // Player is rising
                        if (body.characterMotor.velocity.y < riseSpeed)
                        {
                            //Log.Debug("Falling?: \t" + body.characterMotor.velocity.y + " = " + ((body.characterMotor.velocity.y * dampeningForce) + (riseSpeed * dampeningForce)));
                            //body.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * fallReducedGravity;
                            body.characterMotor.velocity.y = (body.characterMotor.velocity.y - riseSpeed) * dampeningForce + riseSpeed;
                        }
                    }
                }
            }

            public void Start()
            {
                characterMotor = body.characterMotor;
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