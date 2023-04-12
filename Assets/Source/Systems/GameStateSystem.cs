using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class GameStateSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulation_ecbs;

    protected override void OnStartRunning()
    {
        endSimulation_ecbs = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb =  endSimulation_ecbs.CreateCommandBuffer();

        Entities.ForEach((Entity entity, int entityInQueryIndex, in GameStateStart gameStartState) =>
        {
            ecb.RemoveComponent<GameStateStart>(entity);

            ecb.AddComponent<GameStateRunning>(entity);

        }).Schedule();

        endSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
