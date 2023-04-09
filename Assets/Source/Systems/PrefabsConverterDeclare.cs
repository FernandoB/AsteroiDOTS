using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
public class PrefabsConverterDeclare : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((PrefabsReferences prefabReference) =>
        {
            DeclareReferencedPrefab(prefabReference.asteroidPrefab);
            DeclareReferencedPrefab(prefabReference.playerPrefab);
            DeclareReferencedPrefab(prefabReference.bulletPrefab);
        });
    }
}
