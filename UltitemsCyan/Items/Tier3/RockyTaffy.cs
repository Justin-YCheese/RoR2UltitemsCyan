using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;

namespace UltitemsCyan.Items.Tier3
{

    // TODO: check if Item classes needs to be public
    // Fix with transcendance
    // num59 += (float)num4 * 0.08f * this.maxHealth;

    public class RockyTaffy : ItemBase
    {
        public static ItemDef item;
        private const float shieldPercent = 30f;

        // Without shield total barrier decay
        //private const float taffyBarrierDecay = 20f;

        public static GameObject CaptainBodyArmorBlockEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Captain/CaptainBodyArmorBlockEffect.prefab").WaitForCompletion();

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Rocky Taffy";
            if (!CheckItemEnabledConfig(itemName, "Red", configs))
            {
                return;
            }
            item = CreateItemDef(
                "ROCKYTAFFY",
                itemName,
                "Gain a recharging shield. Buff gives a stable barrier without your shield. Buff gained with full shield.",
                "Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>30%</style> <style=cStack>(+30% per stack)</style> of your maximum health. On losing your shield with the buff, gain a <style=cIsHealing>stable barrier</style> for 100% of your <style=cIsHealing>max shield</style>. No barrier decay without a shield and regain buff with a full shield.",
                "This vault is sturdy, but over time the rust will just crack it open. Oh wait this is the wrong description...\n" +
                "Give me a second...\n\num...\n\nSomething about laughing but harder so for shields? I don't know...",
                ItemTier.Tier3,
                UltAssets.RockyTaffySprite,
                UltAssets.RockyTaffyPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility, ItemTag.Healing]
            );
        }

        protected override void Hooks()
        {
            // Add Shield and freeze barrier if no shield
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            // Grant Barrier when losing shield
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            // No Barrier Decay without shield
            On.RoR2.HealthComponent.GetBarrierDecayRate += HealthComponent_GetBarrierDecayRate;
            // Add Recalculate Stats on shield lost / gained
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        private float HealthComponent_GetBarrierDecayRate(On.RoR2.HealthComponent.orig_GetBarrierDecayRate orig, HealthComponent self)
        {
            // Calculate just in case of compatability with other mods?
            //float barrierDecay = orig(self);
            // If you have taffy and no shield
            if (self.body.inventory.GetItemCountEffective(item) > 0 && self.shield <= 0)
            {
                // Then don't lose barrier
                return orig(self) * 0f;
            }
            return orig(self);
        }

        // Add Shield
        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                int grabCount = sender.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    //Log.Debug("Taffy On the rocks | Health: " + sender.healthComponent.fullHealth);
                    args.baseShieldAdd += sender.healthComponent.fullHealth * (shieldPercent / 100f * grabCount);

                    //if (sender.healthComponent.shield <= 0)
                    //{
                    //    //Log.Debug("Freezing my taffy's barrier");
                    //    args.shouldFreezeBarrier = true;
                    //}
                    //else
                    //{
                    //    //Log.Debug("Unfreeze my taffy's barrier");
                    //    args.shouldFreezeBarrier = false;
                    //}
                }
            }
        }
        //*/

        // Grant Barrier on Losing Shield
        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool runOrig = true;

            if (self && self.body && self.body.inventory)
            {
                int grabCount = self.body.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    runOrig = false;

                    //Log.Debug("   Got Taffy!");
                    bool initialShield = self.shield > 0;

                    orig(self, damageInfo);

                    bool newShield = self.shield > 0;

                    if (initialShield && !newShield && self.body.HasBuff(Buffs.TaffyChewBuff.buff))
                    {
                        //Log.Debug("Taffy Shield lost! Gain Barrier");
                        self.body.RemoveBuff(Buffs.TaffyChewBuff.buff);
                        self.AddBarrier(self.fullShield);
                        _ = Util.PlaySound("Play_gup_death", self.body.gameObject);
                        //self.body.statsDirty = true;
                        //self.body.RecalculateStats();
                    }
                }
            }

            if (runOrig)
            {
                orig(self, damageInfo);
            }
        }

        // Add Recalculate Stats on shield lost / gained
        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.inventory)
            {
                _ = self.AddItemBehavior<RockyTaffyBehaviour>(self.inventory.GetItemCountEffective(item));
            }
        }

        // Recalculate stats when losing or gaining shield
        public class RockyTaffyBehaviour : CharacterBody.ItemBehavior
        {
            public HealthComponent healthComponent;
            private bool _hasShield = true;
            private bool _ifFullShield = false; // False so when you first get full shield then can get buff immediantly
            public bool HasShield
            {
                get { return _hasShield; }
                set
                {
                    // If not already the same value
                    if (_hasShield != value)
                    {
                        _hasShield = value;
                        //Log.Debug(_hasShield + " > Sticky Taffy Dirty Stats (has shield changed) | Shield: " + healthComponent.shield + " vs Full? " + healthComponent.fullShield);
                        body.statsDirty = true;
                    }
                }
            }
            public bool IfFullShield
            {
                get { return _ifFullShield; }
                set
                {
                    // If not already the same value
                    if (_ifFullShield != value)
                    {
                        _ifFullShield = value;
                        if (_ifFullShield)
                        {
                            body.AddBuff(Buffs.TaffyChewBuff.buff);
                            _ = Util.PlaySound("Play_captain_m2_tazer_bounce", body.gameObject);
                        }
                    }
                }
            }

            // If player shield stat changed
            public void FixedUpdate()
            {
                if (healthComponent)
                {
                    HasShield = healthComponent.shield > 0;
                    IfFullShield = healthComponent.shield == healthComponent.fullShield;
                }
            }

            public void Start()
            {
                healthComponent = GetComponent<HealthComponent>();
            }

            public void OnDestroy()
            {
                body.RemoveBuff(Buffs.TaffyChewBuff.buff);
                HasShield = true;
                IfFullShield = false;
            }
        }
    }
}