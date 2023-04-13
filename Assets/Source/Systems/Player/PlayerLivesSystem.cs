using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerLivesSystem : SystemBase
{
    private const float outOfThisWorld = 30f;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnUpdate()
    {
        PlayerData player = GetSingleton<PlayerData>();
        Entity gameStateEntity = GetSingletonEntity<GameStateRunning>();

        float dTime = Time.DeltaTime;
        bool wantsToRes = Input.GetKeyDown(KeyCode.Space);

        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Dependency = Entities
            .WithAll<DisabledTag>()
            .WithNone<HitTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref PlayerData player) =>
        {
            player.resCooldownCounter = math.max(player.resCooldownCounter - dTime, 0f);

            if (player.resCooldownCounter <= 0f)
            {
                if (player.lives == 0)
                {
                    pw.AddComponent<GameStateGameOver>(entityInQueryIndex, gameStateEntity);
                }
                else if (wantsToRes)
                {
                    pw.RemoveComponent<DisabledTag>(entityInQueryIndex, entity);

                    translation.Value = float3.zero;
                }
            }

        }).Schedule(Dependency);

        Dependency = Entities
            .WithAll<HitTag>()
            .WithNone<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref PlayerData player, ref Rotation rotation) =>
        {
            pw.RemoveComponent<HitTag>(entityInQueryIndex, entity);
            pw.AddComponent<DisabledTag>(entityInQueryIndex, entity);

            translation.Value = new float3(-outOfThisWorld, outOfThisWorld, 0);

            rotation.Value = quaternion.identity;

            player.currentSpeed = 0f;
            player.direction = float3.zero;
            player.resCooldownCounter = player.resCooldown;
            player.lives = player.lives - 1;

        }).Schedule(Dependency);

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
