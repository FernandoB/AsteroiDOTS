﻿using Unity.Burst;
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

    private EntityQuery bigDisabledNoneDestroyedQuery;
    private EntityQuery bigEnabledQuery;

    private EntityQuery bigBulletHitQuery;

    private EntityQuery mediumDisabledQuery;

    private EntityQuery mediumDivisionQuery;
    private EntityQuery mediumBulletHitQuery;

    private EntityQuery smallDisabledQuery;

    private EntityQuery smallDivisionQuery;
    private EntityQuery smallBulletHitQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        EntityQueryDesc bigDisabledNoneDestroyedDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { ComponentType.ReadOnly<DestroyedTag>() },
            All = new ComponentType[] { typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>(), ComponentType.ReadOnly<DisabledTag>() }
        };
        bigDisabledNoneDestroyedQuery = GetEntityQuery(bigDisabledNoneDestroyedDesc);

        EntityQueryDesc bigEnabledDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { ComponentType.ReadOnly<DestroyedTag>(), ComponentType.ReadOnly<DisabledTag>() },
            All = new ComponentType[] { ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>() }
        };
        bigEnabledQuery = GetEntityQuery(bigEnabledDesc);

        bigBulletHitQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>(), ComponentType.ReadOnly<HitTag>());

        mediumDisabledQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidMediumTag>(), ComponentType.ReadOnly<DisabledTag>());

        mediumDivisionQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidMediumTag>(), ComponentType.ReadOnly<AsteroidDivision>(), ComponentType.ReadOnly<DisabledTag>());
        mediumBulletHitQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidMediumTag>(), ComponentType.ReadOnly<HitTag>());

        smallDisabledQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidSmallTag>(), ComponentType.ReadOnly<DisabledTag>());

        smallDivisionQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidSmallTag>(), ComponentType.ReadOnly<AsteroidDivision>(), ComponentType.ReadOnly<DisabledTag>());
        smallBulletHitQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidSmallTag>(), ComponentType.ReadOnly<HitTag>());
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        uint randomSeed = (uint)(float)(baseTime + Time.ElapsedTime * 100);

        NativeArray<Entity> bigEnabledEntities = bigEnabledQuery.ToEntityArray(Allocator.TempJob);

        UpdateBigDisabled updateBigDisabled = new UpdateBigDisabled()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter(),
            EnabledBigAsteroids = bigEnabledEntities,
            randomSeed = randomSeed
        };

        NativeArray<Entity> mediumDisabledEntities = mediumDisabledQuery.ToEntityArray(Allocator.TempJob);

        UpdateBulletHit updateBigBulletHit = new UpdateBulletHit()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            ScoreCounterHandle = GetComponentTypeHandle<ScoreCounterData>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter(),
            Asteroids = mediumDisabledEntities,
            amountDivision = 2
        };

        UpdateDivision updateMediumDivision = new UpdateDivision()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            AsteroidDivisionTypeHandle = GetComponentTypeHandle<AsteroidDivision>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter(),
            randomSeed = randomSeed
        };

        NativeArray<Entity> smallDisabledEntities = smallDisabledQuery.ToEntityArray(Allocator.TempJob);

        UpdateBulletHit updateMediumBulletHit = new UpdateBulletHit()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            ScoreCounterHandle = GetComponentTypeHandle<ScoreCounterData>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter(),
            Asteroids = smallDisabledEntities,
            amountDivision = 2
        };

        UpdateDivision updateSmallDivision = new UpdateDivision()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            AsteroidDivisionTypeHandle = GetComponentTypeHandle<AsteroidDivision>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter(),
            randomSeed = randomSeed
        };

        NativeArray<Entity> noEntities = new NativeArray<Entity>(0, Allocator.TempJob);

        UpdateBulletHit updateSmallBulletHit = new UpdateBulletHit()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            ScoreCounterHandle = GetComponentTypeHandle<ScoreCounterData>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter(),
            Asteroids = noEntities,
            amountDivision = 0
        };

        Dependency = updateBigDisabled.ScheduleParallel(bigDisabledNoneDestroyedQuery, Dependency);
        Dependency = updateBigBulletHit.ScheduleParallel(bigBulletHitQuery, Dependency);
        Dependency = updateMediumDivision.ScheduleParallel(mediumDivisionQuery, Dependency);
        Dependency = updateMediumBulletHit.ScheduleParallel(mediumBulletHitQuery, Dependency);
        Dependency = updateSmallDivision.ScheduleParallel(smallDivisionQuery, Dependency);
        Dependency = updateSmallBulletHit.ScheduleParallel(smallBulletHitQuery, Dependency);

        Dependency = bigEnabledEntities.Dispose(Dependency);
        Dependency = mediumDisabledEntities.Dispose(Dependency);
        Dependency = smallDisabledEntities.Dispose(Dependency);
        noEntities.Dispose(Dependency);

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
        [ReadOnly]
        public NativeArray<Entity> EnabledBigAsteroids;

        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<AsteroidData> asteroids = chunk.GetNativeArray<AsteroidData>(AsteroidTypeHandle);
            NativeArray<Translation> positions = chunk.GetNativeArray<Translation>(PositionTypeHandle);

            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)((threadIndex + 1) * (chunkIndex + 1) * randomSeed));

            int amountToEnabled = 3 - EnabledBigAsteroids.Length;

            for (int i = 0; i < asteroids.Length && i < amountToEnabled; i++)
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

    private struct UpdateBulletHit : IJobChunk
    {
        public ComponentTypeHandle<Translation> PositionTypeHandle;
        public ComponentTypeHandle<ScoreCounterData> ScoreCounterHandle;
        [ReadOnly]
        public ComponentTypeHandle<AsteroidData> AsteroidTypeHandle;

        public EntityCommandBuffer.ParallelWriter commandBuffer;

        [ReadOnly]
        public NativeArray<Entity> Asteroids;

        public int amountDivision;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<AsteroidData> asteroids = chunk.GetNativeArray<AsteroidData>(AsteroidTypeHandle);
            NativeArray<Translation> positions = chunk.GetNativeArray<Translation>(PositionTypeHandle);
            NativeArray<ScoreCounterData> scores = chunk.GetNativeArray<ScoreCounterData>(ScoreCounterHandle);

            int count = 0;
            int amount = (Asteroids.Length > amountDivision) ? amountDivision : Asteroids.Length;

            for (int i = 0; i < asteroids.Length; i++)
            {
                float3 divisionPos = positions[i].Value;

                positions[i] = new Translation() { Value = new float3(outOfThisWorld, outOfThisWorld, outOfThisWorld) };

                scores[i] = new ScoreCounterData() { scoreCount = scores[i].scoreCount + 1 };

                commandBuffer.RemoveComponent<HitTag>(i, asteroids[i].entity);
                commandBuffer.AddComponent<DisabledTag>(i, asteroids[i].entity);
                commandBuffer.AddComponent<DestroyedTag>(i, asteroids[i].entity);

                for (int j = 0; j < amountDivision; j++)
                {
                    if (count >= amount) break;

                    AsteroidDivision division = new AsteroidDivision();
                    division.position = divisionPos;
                    commandBuffer.AddComponent<AsteroidDivision>(i, Asteroids[count], division);
                    count++;
                }
            }            
        }
    }

    private struct UpdateDivision : IJobChunk
    {
        [Unity.Collections.LowLevel.Unsafe.NativeSetThreadIndex]
        public int threadIndex;

        public ComponentTypeHandle<Translation> PositionTypeHandle;
        [ReadOnly]
        public ComponentTypeHandle<AsteroidData> AsteroidTypeHandle;
        [ReadOnly]
        public ComponentTypeHandle<AsteroidDivision> AsteroidDivisionTypeHandle;
        [ReadOnly]
        public uint randomSeed;

        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<AsteroidData> asteroids = chunk.GetNativeArray<AsteroidData>(AsteroidTypeHandle);
            NativeArray<Translation> positions = chunk.GetNativeArray<Translation>(PositionTypeHandle);
            NativeArray<AsteroidDivision> divisions = chunk.GetNativeArray<AsteroidDivision>(AsteroidDivisionTypeHandle);

            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)((threadIndex + 1) * (chunkIndex + 1) * randomSeed));

            for (int i = 0; i < asteroids.Length; i++)
            {
                positions[i] = new Translation() { Value = divisions[i].position };
                float3 dir = random.NextFloat3Direction();
                dir.z = 0f;
                AsteroidData asteroidData = new AsteroidData();
                asteroidData.direction = dir;
                asteroidData.speed = random.NextFloat(3f, 7f);
                asteroidData.entity = asteroids[i].entity;
                commandBuffer.SetComponent<AsteroidData>(i, asteroids[i].entity, asteroidData);
                commandBuffer.RemoveComponent<DisabledTag>(i, asteroids[i].entity);
                commandBuffer.RemoveComponent<AsteroidDivision>(i, asteroids[i].entity);
            }
        }
    }
}