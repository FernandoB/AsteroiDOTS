using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class DeleteEntitySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        endSimulation_ecbs = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter pw = endSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<DeleteTag>()
            .ForEach((Entity entity, int entityInQueryIndex) =>
            {
                pw.DestroyEntity(entityInQueryIndex, entity);
            }
            ).ScheduleParallel();

        endSimulation_ecbs.AddJobHandleForProducer(this.Dependency);
    }
}
