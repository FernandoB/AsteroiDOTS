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
                asteroidData.random = Unity.Mathematics.Random.CreateFromIndex((uint)entityInQueryIndex);
                translation.Value = startPos;
                pw.AddComponent<DisabledTag>(entityInQueryIndex, e);

            }).ScheduleParallel();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    private static float3 GetRandomPosArea(ref Unity.Mathematics.Random randon, float minX, float maxX, float minY, float maxY)
    {
        return new float3(  randon.NextFloat(minX, maxX) * (randon.NextBool() ? -1f : 1f),
                            randon.NextFloat(minY, maxY) * (randon.NextBool() ? -1f : 1f),
                            0f);
    }

    protected override void OnUpdate()
    {
        //counter -= Time.DeltaTime;
        //if (counter > 0f) return;
        //counter = 1f;

        //EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        //Entities
        //    .WithAll<AsteroidData>()
        //    .ForEach((Entity e, int entityInQueryIndex, ref Translation translation, ref AsteroidData asteroidData) =>
        //    {
        //        Translation tr = new Translation();
        //        tr.Value = GetRandomPosArea(ref asteroidData.random, 6f, 18f, 8, 12.5f);
        //        pw.SetComponent<Translation>(entityInQueryIndex, e, tr);

        //    }).ScheduleParallel();

        //beginSimulation_ecbs.AddJobHandleForProducer(this.Dependency);
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
