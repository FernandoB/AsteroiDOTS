using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class GameStateGameOverSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem simulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate(GetEntityQuery(typeof(GameStateGameOver)));
    }

    protected override void OnStartRunning()
    {
        simulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = simulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in PlayerData player) =>
        {      
            ecb.DestroyEntity(entityInQueryIndex, entity);

        }).ScheduleParallel();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in AsteroidData player) =>
        {
            ecb.DestroyEntity(entityInQueryIndex, entity);

        }).ScheduleParallel();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in BulletData player) =>
        {
            ecb.DestroyEntity(entityInQueryIndex, entity);

        }).ScheduleParallel();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in GameStateGameOver player) =>
        {
            ecb.RemoveComponent<GameStateGameOver>(entityInQueryIndex, entity);
            ecb.AddComponent<GameStateEnd>(entityInQueryIndex, entity);

        }).ScheduleParallel();

        simulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
