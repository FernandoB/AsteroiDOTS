using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class PlayerCreationSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private Entity playerdEntityPrefab;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateStart)));
    }

    protected override void OnStartRunning()
    {
        PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        playerdEntityPrefab = entitiesPrefabs.playerEntityPrefab;
    }

    protected override void OnUpdate()
    {
        beginSimulation_ecbs.CreateCommandBuffer().Instantiate(playerdEntityPrefab);
    }
}
