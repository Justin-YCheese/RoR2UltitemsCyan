using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UltitemsCyan.Equipment
{

    // TODO: check if Item classes needs to be public
    public class JellyJail : EquipmentBase
    {
        // Inflict Slowdown on self?
        public static EquipmentDef equipment;

        //private static GameObject jailPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/VendingMachineProjectile");
        //private static GameObject jailProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VendingMachine/VendingMachineProjectile.prefab").WaitForCompletion();
        //private static readonly GameObject gupPrefab = MasterCatalog.FindMasterPrefab("GupMaster");
        //private static readonly GameObject jailPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VendingMachine/VendingMachineProjectile.prefab").WaitForCompletion();
        public static DeployableSlot jailSlot;
        public static int jailMaxDeployed = 1;
        public static int jailMaxJails = 4;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Jelly Jail";
            if (!CheckItemEnabledConfig(itemName, "Equipment", configs))
            {
                return;
            }

            CreatePrefab();

            equipment = CreateItemDef(
                "JELLYJAIL",
                itemName,
                "Spawn 5 gups",
                "Spawn 5 gups",
                "Alice that freezes forever",
                60f,
                false,
                true,
                true,
                Ultitems.mysterySprite,
                Ultitems.mysteryPrefab
            //Ultitems.Assets.JellyJailSprite,
            //Ultitems.Assets.JellyJailPrefab
            );
        }

        protected override void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        private void CreatePrefab()
        {

            //var vendingMachine = jailPrefab.GetComponent<VendingMachineBehavior>();
            //var instantiateDeployable = jailProjectilePrefab.GetComponent<RoR2.Projectile.ProjectileInstantiateDeployable>();

            //instantiateDeployable.prefab.AddComponent<JellyJailInteraction>();

            // AddComponent<JellyJailInteraction>();    // Contains on interaction code
            //jailPrefab.AddComponent<InteractionProcFilter>();   // I don't know what this does
            //contentPack.networkedObjectPrefabs.Add(jailPrefab);

            //ammoLockerSlot = DeployableAPI.RegisterDeployableSlot((master, multiplier) => ammoLockerMaxDeployed);

            //jailSlot = DeployableAPI.RegisterDeployableSlot((master, multiplier) => ammoLockerMaxDeployed);
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active && self.equipmentDisabled && equipmentDef == equipment)
            {
                Ray ray = new(self.GetAimRay().origin, Vector3.down);
                RaycastHit raycastHit;
                if (Util.CharacterRaycast(self.gameObject, ray, out raycastHit, 1000f, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
                {
                    //RoR2.Projectile.ProjectileManager.instance.FireProjectile(jailProjectilePrefab, raycastHit.point, Quaternion.identity, self.gameObject, self.characterBody.damage, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
                    //self.subcooldownTimer = 0.5f;
                    SpawnJelly(raycastHit.point, Util.QuaternionSafeLookRotation(raycastHit.point));

                    return true;
                }
                return false;

                //self.characterBody.master.AddDeployable
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        private void SpawnJelly(Vector3 point, Quaternion rotation)
        {
            CharacterMaster characterMaster = new MasterSummon
            {
                masterPrefab = MasterCatalog.FindMasterPrefab("BeetleMaster"),
                position = point,
                rotation = rotation,
                summonerBodyObject = null,
                ignoreTeamMemberLimit = true,
                teamIndexOverride = TeamIndex.Monster
            }.Perform();
        }

        /*
        private bool FireVendingMachine()
		{
			Ray ray = new Ray(this.GetAimRay().origin, Vector3.down);
			RaycastHit raycastHit;
			if (Util.CharacterRaycast(base.gameObject, ray, out raycastHit, 1000f, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
			{
				GameObject prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/VendingMachineProjectile");
				ProjectileManager.instance.FireProjectile(prefab, raycastHit.point, Quaternion.identity, base.gameObject, this.characterBody.damage, 0f, Util.CheckRoll(this.characterBody.crit, this.characterBody.master), DamageColorIndex.Default, null, -1f);
				this.subcooldownTimer = 0.5f;
				return true;
			}
			return false;
		}
        */
    }

    /*/AmmoLockerInteraction
    public class JellyJailInteraction : NetworkBehaviour, IInteractable, IDisplayNameProvider
    {
        [SyncVar]
        public bool available = true;

        [SyncVar]
        public NetworkInstanceId characterBodyNetId;

        private void Awake()
        {
            foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                var entityLocator = collider.gameObject.GetComponent<EntityLocator>() ?? collider.gameObject.AddComponent<EntityLocator>();
                entityLocator.entity = gameObject;
            }
        }

        [Server]
        public void SetCharacterBodies(CharacterBody owner, CharacterBody body)
        {
            Log.Debug(string.Format("SetCharacterBodies {0}, {1}", owner, body));
            characterBodyNetId = body.netId;
            //RpcSetPortrait(owner.netId, body.netId);
        }


        public static CharacterBody GetCharacterBody(NetworkInstanceId netId)
        {
            return ClientScene.FindLocalObject(netId)?.GetComponent<CharacterBody>();
        }


        //
        //[ClientRpc]
        //public void RpcSetPortrait(NetworkInstanceId ownerNetId, NetworkInstanceId characterNetId)
        //{
        //    Log.Debug(string.Format("RpcSetPortrait {0}, {1}", ownerNetId, characterNetId));
        //    var ownerBody = GetCharacterBody(ownerNetId);
        //    //var skinTexture = null;
        //    var skinTexture = SkinCatalog.FindCurrentSkinDefForBodyInstance(ownerBody.gameObject);
        //    var body = GetCharacterBody(characterNetId);
        //    foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
        //    {
        //        Log.Debug(string.Format("Renderer on {0}", renderer.gameObject));
        //        var materials = renderer.materials;
        //        foreach (var material in materials)
        //        {
        //            Log.Debug(string.Format("Material {0}", material.name));
        //            if (body.portraitIcon != null && material.name.StartsWith("Portrait"))
        //            {
        //                Log.Debug(string.Format("Setting portrait mainTexture to {0}", body.portraitIcon));
        //                material.mainTexture = body.portraitIcon;
        //            }
        //            else if (skinTexture != null && material.name.StartsWith("Skin"))
        //            {
        //                Log.Debug(string.Format("Setting skin mainTexture to {0}", skinTexture));
        //                //material.mainTexture = skinTexture;
        //            }
        //        }
        //        renderer.materials = materials;
        //    }
        //}
        ///

        public string GetContextString(Interactor activator)//[Not NULL]
        {
            return "Jelly Context";
        }

        public Interactability GetInteractability(Interactor activator)//[Not NULL]
        {
            if (available && activator.GetComponent<CharacterBody>()?.netId == characterBodyNetId)
            {
                return Interactability.Available;
            }
            return Interactability.Disabled;
        }

        public void OnInteractionBegin(Interactor activator) //[Not NULL]
        {
            if (available)
            {
                var characterBody = activator.GetComponent<CharacterBody>();
                if (characterBody != null)
                {
                    available = false;

                    Log.Debug("Hey! an interaction worked!");
                    //characterBody.AddTimedBuffAuthority(AmmoLocker.overchargeDef.buffIndex, AmmoLocker.ammoLockerBuffDuration);
                    //characterBody.AddTimedBuffAuthority(AmmoLocker.shoringDef.buffIndex, AmmoLocker.ammoLockerBuffDuration);
                    characterBody.healthComponent.AddBarrierAuthority(characterBody.healthComponent.fullBarrier * .25f);

                    RpcOnInteraction(characterBody.netId);
                }
            }
        }

        [ClientRpc]
        public void RpcOnInteraction(NetworkInstanceId bodyId)
        {
            Log.Debug("Jelly RpcOnInteraction");

            //var skillLocator = GetCharacterBody(bodyId)?.skillLocator;
            //if (skillLocator != null)
            //{
            //    foreach (var skill in skillLocator.allSkills)
            //    {
            //        skill.Reset();
            //    }
            //}

            gameObject.GetComponentInChildren<Animator>().Play("Base Layer.Opening");
        }

        public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
        {
            return false;
        }

        public bool ShouldShowOnScanner()
        {
            return false;
        }

        public string GetDisplayName()
        {
            return "Jelly_Jail_Display_Name";
        }
    }
    //*/
}