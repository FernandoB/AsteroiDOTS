using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AlienShipCreationSystem : SystemBase
{
    private const float outOfThisWorld = 30f;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;


    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateStart)));
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        EntityCommandBuffer ecb = beginSimulation_ecbs.CreateCommandBuffer();

        float3 startPos = new float3(-outOfThisWorld, -outOfThisWorld, 0);

        Entity entityBig = ecb.Instantiate(entitiesPrefabs.alienShipBigEntityPrefab);
        ecb.SetComponent<Translation>(entityBig, new Translation() { Value = startPos });
        ecb.AddComponent<DisabledTag>(entityBig);
        ecb.AddComponent<ScoreCounterData>(entityBig, new ScoreCounterData() { scoreCount = 0, score = 200 });

        Entity entitySmall = ecb.Instantiate(entitiesPrefabs.alienShipSmallEntityPrefab);
        ecb.SetComponent<Translation>(entitySmall, new Translation() { Value = startPos });
        ecb.AddComponent<DisabledTag>(entitySmall);
        ecb.AddComponent<ScoreCounterData>(entitySmall, new ScoreCounterData() { scoreCount = 0, score = 200 });
    }

    protected override void OnDestroy()
    {

    }

    protected override void OnStopRunning()
    {

    }
}
