using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class PlayerCreationSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        Entity playerdEntity = EntityManager.Instantiate(entitiesPrefabs.playerEntityPrefab);
    }

    protected override void OnUpdate()
    {

    }
}
