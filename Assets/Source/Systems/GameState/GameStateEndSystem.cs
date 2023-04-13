using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class GameStateEndSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateEnd)));
    }

    protected override void OnStartRunning()
    {
            
    }

    protected override void OnUpdate()
    {
        MainGame.Instance.GameEnd();

        EntityCommandBuffer.ParallelWriter ecb = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in GameStateEnd gameState) =>
        {
            ecb.RemoveComponent<GameStateEnd>(entityInQueryIndex, entity);
            ecb.DestroyEntity(entityInQueryIndex, entity);

        }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
