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
            Entity prefab = GetPrimaryEntity(prefabReference.asteroidPrefab);

            var component = new PrefabsEntitiesReferences { asteroidEntityPrefab = prefab };
            DstEntityManager.AddComponentData(entity, component);
        });
    }
}
