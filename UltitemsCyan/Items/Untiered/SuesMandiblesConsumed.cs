using BepInEx.Configuration;
using RoR2;

namespace UltitemsCyan.Items.Untiered
{

    // TODO: check if Item classes needs to be public
    public class SuesMandiblesConsumed : ItemBase
    {
        public static ItemDef item;

        public override void Init(ConfigFile configs)
        {
            item = CreateItemDef(
                "SUESMANDIBLESCONSUMED",
                "Sue's Mandibles (Consumed)",
                "Resting in pieces",
                "DESCRIPTION Resting in pieces",
                "I don't know sue",
                ItemTier.NoTier,
                UltAssets.SuesMandiblesConsumedSprite,
                UltAssets.SuesMandiblesConsumedPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility, ItemTag.LowHealth, ItemTag.AIBlacklist, ItemTag.AllowedForUseAsCraftingIngredient],
                null,
                true
            );
        }

        protected override void Hooks() { }
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