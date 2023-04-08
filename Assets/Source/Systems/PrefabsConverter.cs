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
            Entity asteroidPrefab = GetPrimaryEntity(prefabReference.asteroidPrefab);
            Entity playerPrefab = GetPrimaryEntity(prefabReference.playerPrefab);

            PrefabsEntitiesReferences component = new PrefabsEntitiesReferences
            {
                asteroidEntityPrefab = asteroidPrefab,
                playerEntityPrefab = playerPrefab
            };

            DstEntityManager.AddComponentData(entity, component);
        });
    }
}
