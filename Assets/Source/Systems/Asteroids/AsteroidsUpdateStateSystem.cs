using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsUpdateStateSystem : SystemBase
{
    private const float outOfThisWorld = 30f;
    private double baseTime = System.DateTime.Now.TimeOfDay.TotalSeconds;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private EntityQuery bigDisabledQuery;
    private EntityQuery bigBulletHitQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        bigDisabledQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>(), ComponentType.ReadOnly<DisabledTag>());
        bigBulletHitQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>(), ComponentType.ReadOnly<BulletHitTag>());
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        uint randomSeed = (uint)(float)(baseTime + Time.ElapsedTime * 100);

        UpdateBigDisabled updateBigDisabled = new UpdateBigDisabled()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter(),
            randomSeed = randomSeed
        };

        UpdateBigBulletHit updateBigBulletHit = new UpdateBigBulletHit()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter()
        };

        Dependency = updateBigDisabled.ScheduleParallel(bigDisabledQuery, Dependency);
        Dependency = updateBigBulletHit.ScheduleParallel(bigBulletHitQuery, Dependency);

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    protected override void OnDestroy()
    {

    }

    protected override void OnStopRunning()
    {

    }

    private static float3 GetRandomPosArea(ref Unity.Mathematics.Random randon, float minX, float maxX, float minY, float maxY)
    {
        return new float3(randon.NextFloat(minX, maxX) * (randon.NextBool() ? -1f : 1f),
                            randon.NextFloat(minY, maxY) * (randon.NextBool() ? -1f : 1f),
                            0f);
    }

    private struct UpdateBigDisabled : IJobChunk
    {
        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex]
        public int threadIndex;

        public ComponentTypeHandle<Translation> PositionTypeHandle;
        [ReadOnly]
        public ComponentTypeHandle<AsteroidData> AsteroidTypeHandle;
        [ReadOnly]
        public uint randomSeed;

        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<AsteroidData> asteroids = chunk.GetNativeArray<AsteroidData>(AsteroidTypeHandle);
            NativeArray<Translation> positions = chunk.GetNativeArray<Translation>(PositionTypeHandle);

            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)((threadIndex + 1) * (chunkIndex + 1) * randomSeed));

            for (int i = 0; i < asteroids.Length; i++)
            {
                positions[i] = new Translation() { Value = GetRandomPosArea(ref random, 6f, 18f, 8f, 12.5f) };
                float3 dir = random.NextFloat3Direction();
                dir.z = 0f;
                AsteroidData asteroidData = new AsteroidData();
                asteroidData.direction = dir;
                asteroidData.speed = random.NextFloat(2f, 6f);
                asteroidData.entity = asteroids[i].entity;
                commandBuffer.SetComponent<AsteroidData>(i, asteroids[i].entity, asteroidData);
                commandBuffer.RemoveComponent<DisabledTag>(i, asteroids[i].entity);
            }
        }
    }

    private struct UpdateBigBulletHit : IJobChunk
    {
        public ComponentTypeHandle<Translation> PositionTypeHandle;
        [ReadOnly]
        public ComponentTypeHandle<AsteroidData> AsteroidTypeHandle;

        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<AsteroidData> asteroids = chunk.GetNativeArray<AsteroidData>(AsteroidTypeHandle);
            NativeArray<Translation> positions = chunk.GetNativeArray<Translation>(PositionTypeHandle);

            for (int i = 0; i < asteroids.Length; i++)
            {
                positions[i] = new Translation() { Value = new float3(outOfThisWorld, outOfThisWorld, outOfThisWorld) };
                commandBuffer.RemoveComponent<BulletHitTag>(i, asteroids[i].entity);
                commandBuffer.AddComponent<DisabledTag>(i, asteroids[i].entity);
            }
        }
    }
}