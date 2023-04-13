using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class GameStateSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem simulation_ecbs;

    protected override void OnStartRunning()
    {
        simulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb =  simulation_ecbs.CreateCommandBuffer();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in GameStateStart gameStartState) =>
        {
            ecb.RemoveComponent<GameStateStart>(entity);

            ecb.AddComponent<GameStateRunning>(entity);

        }).Schedule();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in GameStateRunning gameStartState, in GameStateGameOver gameOverState) =>
        {
            ecb.RemoveComponent<GameStateRunning>(entity);

        }).Schedule();

        simulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
