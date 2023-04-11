using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsCreationSystem : SystemBase
{
    private NativeArray<Entity> asteroidsBig;
    private NativeArray<Entity> asteroidsMedium;
    private NativeArray<Entity> asteroidsSmall;

    private const int maxBigAsteroids = 3;

    private const float outOfThisWorld = 30f;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private float counter = 1f;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        asteroidsBig = EntityManager.Instantiate(entitiesPrefabs.asteroidBigEntityPrefab, maxBigAsteroids, Allocator.Persistent);
        asteroidsMedium = EntityManager.Instantiate(entitiesPrefabs.asteroidMediumEntityPrefab, maxBigAsteroids * 2, Allocator.Persistent);
        asteroidsSmall = EntityManager.Instantiate(entitiesPrefabs.asteroidSmallEntityPrefab, maxBigAsteroids * 2 * 2, Allocator.Persistent);

        float3 startPos = new float3(outOfThisWorld, outOfThisWorld, outOfThisWorld);

        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<AsteroidData>()
            .ForEach((Entity e, int entityInQueryIndex, ref Translation translation, ref AsteroidData asteroidData) =>
            {
                translation.Value = startPos;
                asteroidData.entity = e;
                pw.AddComponent<DisabledTag>(entityInQueryIndex, e);

            }).ScheduleParallel();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    protected override void OnUpdate()
    {

    }

    protected override void OnDestroy()
    {

    }

    protected override void OnStopRunning()
    {
        asteroidsBig.Dispose();
        asteroidsMedium.Dispose();
        asteroidsSmall.Dispose();
    }
}
