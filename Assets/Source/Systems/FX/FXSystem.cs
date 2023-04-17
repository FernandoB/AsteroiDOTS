using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class FXSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = beginSimulation_ecbs.CreateCommandBuffer();

        Entities
            .WithoutBurst()
            .ForEach((Entity entity, int entityInQueryIndex, in FXData data) =>
        {
            MainGame.Instance.SetFX(data.fxId, data.posX, data.posY);

            ecb.DestroyEntity(entity);

        }).Run();

    }
}
