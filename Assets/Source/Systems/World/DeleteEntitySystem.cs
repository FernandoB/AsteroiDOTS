using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class DeleteEntitySystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter pw = ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<DeleteTag>()
            .ForEach((Entity entity, int entityInQueryIndex) =>
            {
                pw.DestroyEntity(entityInQueryIndex, entity);
            }
            ).ScheduleParallel();

        ecbs.AddJobHandleForProducer(this.Dependency);
    }
}
