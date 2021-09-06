using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.Modules
{
    internal static class Projectiles
    {
        internal static GameObject bombPrefab;
        internal static GameObject conePrefab;

        internal static void RegisterProjectiles()
        {
            // only separating into separate methods for my sanity

            CreateBomb();
            CreateCone();
            AddProjectile(conePrefab);
            AddProjectile(bombPrefab);
        }

        internal static void AddProjectile(GameObject projectileToAdd)
        {
            Modules.Prefabs.projectilePrefabs.Add(projectileToAdd);
        }

        private static void CreateBomb()
        {
            bombPrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", "SettBombProjectile");

            ProjectileImpactExplosion bombImpactExplosion = bombPrefab.GetComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(bombImpactExplosion);

            bombImpactExplosion.blastRadius = 16f;
            bombImpactExplosion.destroyOnEnemy = true;
            bombImpactExplosion.lifetime = 12f;
            bombImpactExplosion.impactEffect = Modules.Assets.bombExplosionEffect;
            //bombImpactExplosion.lifetimeExpiredSound = Modules.Assets.CreateNetworkSoundEventDef("SettBombExplosion");
            bombImpactExplosion.timerAfterImpact = true;
            bombImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController bombController = bombPrefab.GetComponent<ProjectileController>();
            if (Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("SettBombGhost") != null) bombController.ghostPrefab = CreateGhostPrefab("SettBombGhost");
            bombController.startSound = "";
        }

        private static void CreateCone()
        {
            conePrefab = CloneProjectilePrefab("LoaderZapCone", "SettconeProjectile");

            ProjectileProximityBeamController projectileProximityBeamController = conePrefab.GetComponent<ProjectileProximityBeamController>();

            projectileProximityBeamController.damageCoefficient = 1f;
        }

        private static void InitializeImpactExplosion(ProjectileImpactExplosion projectileImpactExplosion)
        {
            projectileImpactExplosion.blastDamageCoefficient = 1f;
            projectileImpactExplosion.blastProcCoefficient = 1f;
            projectileImpactExplosion.blastRadius = 1f;
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.childrenDamageCoefficient = 0f;
            projectileImpactExplosion.childrenProjectilePrefab = null;
            projectileImpactExplosion.destroyOnEnemy = false;
            projectileImpactExplosion.destroyOnWorld = false;
#pragma warning disable CS0618 // 'ProjectileExplosion.explosionSoundString' is obsolete: 'This sound will not play over the network. Provide the sound via the prefab referenced by explosionEffect instead.'
            projectileImpactExplosion.explosionSoundString = "";
#pragma warning restore CS0618 // 'ProjectileExplosion.explosionSoundString' is obsolete: 'This sound will not play over the network. Provide the sound via the prefab referenced by explosionEffect instead.'
            projectileImpactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            projectileImpactExplosion.fireChildren = false;
            projectileImpactExplosion.impactEffect = null;
            projectileImpactExplosion.lifetime = 0f;
            projectileImpactExplosion.lifetimeAfterImpact = 0f;
#pragma warning disable CS0618 // 'ProjectileImpactExplosion.lifetimeExpiredSoundString' is obsolete: 'This sound will not play over the network. Use lifetimeExpiredSound instead.'
            projectileImpactExplosion.lifetimeExpiredSoundString = "";
#pragma warning restore CS0618 // 'ProjectileImpactExplosion.lifetimeExpiredSoundString' is obsolete: 'This sound will not play over the network. Use lifetimeExpiredSound instead.'
            projectileImpactExplosion.lifetimeRandomOffset = 0f;
            projectileImpactExplosion.offsetForLifetimeExpiredSound = 0f;
            projectileImpactExplosion.timerAfterImpact = false;

            projectileImpactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
        }

        private static GameObject CreateGhostPrefab(string ghostName)
        {
            GameObject ghostPrefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>(ghostName);
            if (!ghostPrefab.GetComponent<NetworkIdentity>()) ghostPrefab.AddComponent<NetworkIdentity>();
            if (!ghostPrefab.GetComponent<ProjectileGhostController>()) ghostPrefab.AddComponent<ProjectileGhostController>();

            Modules.Assets.ConvertAllRenderersToHopooShader(ghostPrefab);

            return ghostPrefab;
        }

        private static GameObject CloneProjectilePrefab(string prefabName, string newPrefabName)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/" + prefabName), newPrefabName);
            return newPrefab;
        }
    }
}