using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AlienShipMoveSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        float deltaTime = Time.DeltaTime;

        Entities
            .WithNone<DisabledTag>()
            .ForEach((ref Translation translation, in AlienShipData alienShipData) =>
            {
                translation.Value = translation.Value + (alienShipData.direction * alienShipData.speed * deltaTime);

            }).ScheduleParallel();

        Entities
            .WithNone<DisabledTag>()
            .WithAll<HitTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in AlienShipData alienShipData) =>
            {
                pw.AddComponent<DisabledTag>(entityInQueryIndex, entity);
                pw.RemoveComponent<HitTag>(entityInQueryIndex, entity);

            }).ScheduleParallel();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
