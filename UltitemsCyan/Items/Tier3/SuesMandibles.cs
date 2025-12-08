using RoR2;
using UltitemsCyan.Items.Untiered;
using BepInEx.Configuration;
using UltitemsCyan.Buffs;

namespace UltitemsCyan.Items.Tier3
{

    // TODO: check if Item classes needs to be public
    public class SuesMandibles : ItemBase
    {
        public static ItemDef item;
        //private const float warningDelay = 25f;
        private const float effectDuration = 30f;

        public override void Init(ConfigFile configs)
        {
            if (!CheckItemEnabledConfig("Sues Mandibles", "Red", configs)) // Can't have apostrophes
            {
                return;
            }
            item = CreateItemDef(
                "SUESMANDIBLES",
                "Sue's Mandibles",
                "Endure a killing blow then gain invulnerability and disable healing for 30s. Consumed on use.",
                "<style=cIsUtility>Upon a killing blow</style>, this item will be <style=cIsUtility>consumed</style> and you'll <style=cIsHealing>live on 1 health</style> with <style=cIsHealing>30 seconds</style> of <style=cIsHealing>invulnerability</style> and <style=cIsHealth>disabled healing</style>.",
                "Last Stand",
                ItemTier.Tier3,
                UltAssets.SuesMandiblesSprite,
                UltAssets.SuesMandiblesPrefab,
                [ItemTag.Utility, ItemTag.ExtractorUnitBlacklist]
            );
        }


        protected override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            CharacterBody victim = self.body;
            // If dead after damage
            if (victim && victim.inventory && self && !self.alive && self.health <= 0)
            {
                int grabCount = victim.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    Log.Warning(" ! ! ! Killing Blow ! ! ! ");
                    //Log.Debug("S Teeth Combined: " + self.combinedHealth + " FullCombined: " + self.fullCombinedHealth + " Damage: " + damageInfo.damage + " Alive? " + self.alive);

                    // Consume Item
                    // TODO test if discarding with '_' actually works
                    Inventory.ItemTransformation.TryTransformResult tryTransformResult;
                    if (new Inventory.ItemTransformation
                    {
                        originalItemIndex = item.itemIndex,
                        newItemIndex = SuesMandiblesConsumed.item.itemIndex,
                        maxToTransform = 1,
                        transformationType = 0
                    }.TryTransform(victim.inventory, out tryTransformResult))
                    {
                        // If item succesfully transformed
                        Log.Warning(" Sue's saved your ! ! ! Killing Blow ! ! ! ");

                        // Regain one health
                        self.health = 1;

                        // Sue's Teeth timer for duration
                        for (int i = 1; i <= effectDuration; i++)
                        {
                            victim.AddTimedBuffAuthority(SuesTeethBuff.buff.buffIndex, i);
                        }
                        victim.AddTimedBuffAuthority(RoR2Content.Buffs.Immune.buffIndex, effectDuration);
                        victim.AddTimedBuffAuthority(RoR2Content.Buffs.HealingDisabled.buffIndex, effectDuration); // Adds synergy with Ben's Raincoat and Genisis Loop

                        // Play Sounds
                        _ = Util.PlaySound("Play_item_proc_ghostOnKill", victim.gameObject);
                        _ = Util.PlaySound("Play_item_proc_ghostOnKill", victim.gameObject);
                        _ = Util.PlaySound("Play_item_proc_phasing", victim.gameObject);
                        _ = Util.PlaySound("Play_item_proc_phasing", victim.gameObject);
                        _ = Util.PlaySound("Play_elite_haunt_ghost_convert", victim.gameObject);
                    }
                }
            }
            //Log.Debug("Bye Sue");
        }
    }
    /*/
        public void RespawnExtraLife()
		{
			this.inventory.GiveItem(RoR2Content.Items.ExtraLifeConsumed, 1);
			CharacterMasterNotificationQueue.SendTransformNotification(this, RoR2Content.Items.ExtraLife.itemIndex, RoR2Content.Items.ExtraLifeConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
			Vector3 vector = this.deathFootPosition;
			if (this.killedByUnsafeArea)
			{
				vector = (TeleportHelper.FindSafeTeleportDestination(this.deathFootPosition, this.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng) ?? this.deathFootPosition);
			}
			this.Respawn(vector, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
			this.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
			GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
			if (this.bodyInstanceObject)
			{
				foreach (EntityStateMachine entityStateMachine in this.bodyInstanceObject.GetComponents<EntityStateMachine>())
				{
					entityStateMachine.initialStateType = entityStateMachine.mainStateType;
				}
				if (gameObject)
				{
					EffectManager.SpawnEffect(gameObject, new EffectData
					{
						origin = vector,
						rotation = this.bodyInstanceObject.transform.rotation
					}, true);
				}
			}
		}
    //*/
}