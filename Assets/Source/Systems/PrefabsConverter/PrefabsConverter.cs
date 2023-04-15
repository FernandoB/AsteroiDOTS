using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class PrefabsConverter : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((PrefabsReferences prefabReference) =>
        {
            Entity entity = GetPrimaryEntity(prefabReference);

            PrefabsEntitiesReferences component = new PrefabsEntitiesReferences
            {
                asteroidBigEntityPrefab = GetPrimaryEntity(prefabReference.asteroidBigPrefab),
                asteroidMediumEntityPrefab = GetPrimaryEntity(prefabReference.asteroidMediumPrefab),
                asteroidSmallEntityPrefab = GetPrimaryEntity(prefabReference.asteroidSmallPrefab),
                playerEntityPrefab = GetPrimaryEntity(prefabReference.playerPrefab),
                bulletEntityPrefab = GetPrimaryEntity(prefabReference.bulletPrefab),
                alienShipBigEntityPrefab = GetPrimaryEntity(prefabReference.alienShipBigPrefab),
                alienShipSmallEntityPrefab = GetPrimaryEntity(prefabReference.alienShipSmallPrefab),
                powerUpShieldEntityPrefab = GetPrimaryEntity(prefabReference.powerupShield),
                powerupWeaponAEntityPrefab = GetPrimaryEntity(prefabReference.powerupWeaponA)
            };

            DstEntityManager.AddComponentData(entity, component);
        });
    }
}
