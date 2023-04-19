using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsUpdateStateSystem : SystemBase
{
    private const float outOfThisWorld = 300f;
    private double baseTime = System.DateTime.Now.TimeOfDay.TotalSeconds;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private EntityQuery bigEnabledQuery;
    private EntityQuery mediumEnabledQuery;
    private EntityQuery smallEnabledQuery;

    private EntityQuery bigDisabledNoneDestroyedQuery;    

    private EntityQuery bigBulletHitQuery;

    private EntityQuery mediumDisabledQuery;

    private EntityQuery mediumDivisionQuery;
    private EntityQuery mediumBulletHitQuery;

    private EntityQuery smallDisabledQuery;

    private EntityQuery smallDivisionQuery;
    private EntityQuery smallBulletHitQuery;

    private EntityQuery alienShipDisabledQuery;

    private bool nextBigWave = false;
    private bool prevNextBigWave = false;
    private bool posibleNextWave = false;
    private int nextWaveFrameCounter = 0;

    private int waveAmountAsteroids = 0;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        EntityQueryDesc bigDisabledDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>(), ComponentType.ReadOnly<DisabledTag>() }
        };
        bigDisabledNoneDestroyedQuery = GetEntityQuery(bigDisabledDesc);

        EntityQueryDesc bigEnabledDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { ComponentType.ReadOnly<DisabledTag>() },
            All = new ComponentType[] { ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>() }
        };
        bigEnabledQuery = GetEntityQuery(bigEnabledDesc);

        EntityQueryDesc mediumEnabledDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { ComponentType.ReadOnly<DisabledTag>() },
            All = new ComponentType[] { ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidMediumTag>() }
        };
        mediumEnabledQuery = GetEntityQuery(mediumEnabledDesc);

        EntityQueryDesc smallEnabledDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { ComponentType.ReadOnly<DisabledTag>() },
            All = new ComponentType[] { ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidSmallTag>() }
        };
        smallEnabledQuery = GetEntityQuery(smallEnabledDesc);

        bigBulletHitQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>(), ComponentType.ReadOnly<HitTag>());

        mediumDisabledQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidMediumTag>(), ComponentType.ReadOnly<DisabledTag>());

        mediumDivisionQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidMediumTag>(), ComponentType.ReadOnly<AsteroidDivision>(), ComponentType.ReadOnly<DisabledTag>());
        mediumBulletHitQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidMediumTag>(), ComponentType.ReadOnly<HitTag>());

        smallDisabledQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidSmallTag>(), ComponentType.ReadOnly<DisabledTag>());

        smallDivisionQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidSmallTag>(), ComponentType.ReadOnly<AsteroidDivision>(), ComponentType.ReadOnly<DisabledTag>());
        smallBulletHitQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidSmallTag>(), ComponentType.ReadOnly<HitTag>());

        EntityQueryDesc alienShipEnableddDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { ComponentType.ReadOnly<DisabledTag>() },
            All = new ComponentType[] { ComponentType.ReadOnly<AlienShipData>() }
        };
        alienShipDisabledQuery = GetEntityQuery(alienShipEnableddDesc);
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        uint randomSeed = (uint)(float)(baseTime + Time.ElapsedTime * 100);

        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        prevNextBigWave = nextBigWave;
        nextBigWave = bigEnabledQuery.IsEmpty && mediumEnabledQuery.IsEmpty && smallEnabledQuery.IsEmpty && alienShipDisabledQuery.IsEmpty;
        if (posibleNextWave)
        {
            nextWaveFrameCounter--;
            if(nextWaveFrameCounter <= 0)
            {
                posibleNextWave = false;

                if (nextBigWave)
                {
                    waveAmountAsteroids += 2;

                    UpdateBigDisabled updateBigDisabled = new UpdateBigDisabled()
                    {
                        PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
                        AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
                        commandBuffer = pw,
                        amountToEnable = waveAmountAsteroids,
                        randomSeed = randomSeed
                    };
                    Dependency = updateBigDisabled.ScheduleParallel(bigDisabledNoneDestroyedQuery, Dependency);
                }
            }
        }
        else
        {            
            if (prevNextBigWave != nextBigWave)
            {
                posibleNextWave = true;
                nextWaveFrameCounter = 5;
            }
        }

        NativeArray<Entity> mediumDisabledEntities = mediumDisabledQuery.ToEntityArray(Allocator.TempJob);

        UpdateBulletHit updateBigBulletHit = new UpdateBulletHit()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            ScoreCounterHandle = GetComponentTypeHandle<ScoreCounterData>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = pw,
            Asteroids = mediumDisabledEntities,
            amountDivision = 2
        };

        UpdateDivision updateMediumDivision = new UpdateDivision()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            AsteroidDivisionTypeHandle = GetComponentTypeHandle<AsteroidDivision>(true),
            commandBuffer = pw,
            randomSeed = randomSeed
        };

        NativeArray<Entity> smallDisabledEntities = smallDisabledQuery.ToEntityArray(Allocator.TempJob);

        UpdateBulletHit updateMediumBulletHit = new UpdateBulletHit()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            ScoreCounterHandle = GetComponentTypeHandle<ScoreCounterData>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = pw,
            Asteroids = smallDisabledEntities,
            amountDivision = 2
        };

        UpdateDivision updateSmallDivision = new UpdateDivision()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            AsteroidDivisionTypeHandle = GetComponentTypeHandle<AsteroidDivision>(true),
            commandBuffer = pw,
            randomSeed = randomSeed
        };

        NativeArray<Entity> noEntities = new NativeArray<Entity>(0, Allocator.TempJob);

        UpdateBulletHit updateSmallBulletHit = new UpdateBulletHit()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            ScoreCounterHandle = GetComponentTypeHandle<ScoreCounterData>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = pw,
            Asteroids = noEntities,
            amountDivision = 0
        };

        Dependency = updateBigBulletHit.ScheduleParallel(bigBulletHitQuery, Dependency);
        Dependency = updateMediumDivision.ScheduleParallel(mediumDivisionQuery, Dependency);
        Dependency = updateMediumBulletHit.ScheduleParallel(mediumBulletHitQuery, Dependency);
        Dependency = updateSmallDivision.ScheduleParallel(smallDivisionQuery, Dependency);
        Dependency = updateSmallBulletHit.ScheduleParallel(smallBulletHitQuery, Dependency);

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
        waveAmountAsteroids = 0;
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

        public int amountToEnable;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<AsteroidData> asteroids = chunk.GetNativeArray<AsteroidData>(AsteroidTypeHandle);
            NativeArray<Translation> positions = chunk.GetNativeArray<Translation>(PositionTypeHandle);

            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)((threadIndex + 1) * (chunkIndex + 1) * randomSeed));

            for (int i = 0; i < asteroids.Length && i < amountToEnable; i++)
            {
                positions[i] = new Translation() { Value = Utils.GetRandomPosArea(ref random, 6f, 18f, 8f, 12.5f) };
                float3 dir = random.NextFloat3Direction();
                dir.z = 0f;
                AsteroidData asteroidData = new AsteroidData();
                asteroidData.direction = dir;
                asteroidData.speed = random.NextFloat(2f, 6f);
                asteroidData.entity = asteroids[i].entity;
                asteroidData.hitFx = asteroids[i].hitFx;
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

                positions[i] = new Translation() { Value = new float3(outOfThisWorld, outOfThisWorld, 0) };

                ScoreCounterData sc = scores[i];
                sc.scoreCount = sc.scoreCount + 1;
                scores[i] = sc;

                commandBuffer.RemoveComponent<HitTag>(i, asteroids[i].entity);
                commandBuffer.AddComponent<DisabledTag>(i, asteroids[i].entity);

                for (int j = 0; j < amountDivision; j++)
                {
                    if (count >= amount) break;

                    AsteroidDivision division = new AsteroidDivision();
                    division.position = divisionPos;
                    commandBuffer.AddComponent<AsteroidDivision>(i, Asteroids[count], division);
                    count++;
                }

                Entity fxEntity = commandBuffer.CreateEntity(i);
                commandBuffer.AddComponent<FXData>(i, fxEntity, new FXData() { fxId = asteroids[i].hitFx });

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
                asteroidData.hitFx = asteroids[i].hitFx;
                commandBuffer.SetComponent<AsteroidData>(i, asteroids[i].entity, asteroidData);
                commandBuffer.RemoveComponent<DisabledTag>(i, asteroids[i].entity);
                commandBuffer.RemoveComponent<AsteroidDivision>(i, asteroids[i].entity);
            }
        }
    }
}