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

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private EntityQuery bigDisabledQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        bigDisabledQuery = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<AsteroidBigTag>(), ComponentType.ReadOnly<DisabledTag>());
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        UpdateBigDisabled updateBigDisabled = new UpdateBigDisabled()
        {
            PositionTypeHandle = GetComponentTypeHandle<Translation>(false),
            AsteroidTypeHandle = GetComponentTypeHandle<AsteroidData>(true),
            commandBuffer = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter()
        };

        Dependency = updateBigDisabled.ScheduleParallel(bigDisabledQuery, Dependency);

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    protected override void OnDestroy()
    {

    }

    protected override void OnStopRunning()
    {

    }

    private static float3 GetRandomPosArea(Unity.Mathematics.Random randon, float minX, float maxX, float minY, float maxY)
    {
        return new float3(randon.NextFloat(minX, maxX) * (randon.NextBool() ? -1f : 1f),
                            randon.NextFloat(minY, maxY) * (randon.NextBool() ? -1f : 1f),
                            0f);
    }

    private struct UpdateBigDisabled : IJobChunk
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
                Translation newPos = new Translation() { Value = GetRandomPosArea(asteroids[i].random, 6f, 18f, 8f, 12.5f) };
                positions[i] = newPos;
                commandBuffer.RemoveComponent<DisabledTag>(i, asteroids[i].entity);
            }
        }
    }
}