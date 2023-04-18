using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerFollowPosSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        Entities
            .WithAll<PlayerData>()
            .WithNone<DisabledTag>()
            .WithoutBurst()
            .ForEach((Entity entity, int entityInQueryIndex, in Translation translation) =>
            {
                MainGame.Instance.SetPlayerPos(translation.Value.x, translation.Value.y);

            }).Run();

    }
}
