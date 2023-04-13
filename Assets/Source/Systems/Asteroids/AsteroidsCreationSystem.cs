using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsCreationSystem : SystemBase
{
    //private NativeArray<Entity> asteroidsBig;
    //private NativeArray<Entity> asteroidsMedium;
    //private NativeArray<Entity> asteroidsSmall;

    private const int maxBigAsteroids = 5;

    private const float outOfThisWorld = 30f;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private float counter = 1f;

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

        float3 startPos = new float3(outOfThisWorld, outOfThisWorld, 0);

        for (int i = 0; i < maxBigAsteroids; i++)
        {
            Entity e = ecb.Instantiate(entitiesPrefabs.asteroidBigEntityPrefab);
            ecb.SetComponent<Translation>(e, new Translation() { Value = startPos } );
            ecb.SetComponent<AsteroidData>(e, new AsteroidData() { entity = e });
            ecb.AddComponent<DisabledTag>(e);
            ecb.AddComponent<ScoreCounterData>(e, new ScoreCounterData() { scoreCount = 0 });
        }

        for (int i = 0; i < maxBigAsteroids * 2; i++)
        {
            Entity e = ecb.Instantiate(entitiesPrefabs.asteroidMediumEntityPrefab);
            ecb.SetComponent<Translation>(e, new Translation() { Value = startPos });
            ecb.SetComponent<AsteroidData>(e, new AsteroidData() { entity = e });
            ecb.AddComponent<DisabledTag>(e);
            ecb.AddComponent<ScoreCounterData>(e, new ScoreCounterData() { scoreCount = 0 });
        }

        for (int i = 0; i < maxBigAsteroids * 2 * 2; i++)
        {
            Entity e = ecb.Instantiate(entitiesPrefabs.asteroidSmallEntityPrefab);
            ecb.SetComponent<Translation>(e, new Translation() { Value = startPos });
            ecb.SetComponent<AsteroidData>(e, new AsteroidData() { entity = e });
            ecb.AddComponent<DisabledTag>(e);
            ecb.AddComponent<ScoreCounterData>(e, new ScoreCounterData() { scoreCount = 0 });
        }
    }

    protected override void OnDestroy()
    {

    }

    protected override void OnStopRunning()
    {

    }
}
