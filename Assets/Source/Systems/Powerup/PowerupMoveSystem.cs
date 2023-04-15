using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PowerMoveSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<BulletData>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref BulletData bulletData) =>
            {
                translation.Value = translation.Value + ((bulletData.startVelocity + (bulletData.direction * bulletData.maxSpeed)) * deltaTime);

                bulletData.lifeTime -= deltaTime;
                if (bulletData.lifeTime < 0f)
                {
                    pw.AddComponent<DeleteTag>(entityInQueryIndex, entity);
                }

            }).ScheduleParallel();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
