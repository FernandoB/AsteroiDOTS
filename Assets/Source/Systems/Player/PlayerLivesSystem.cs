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

    private bool prevReadyToRes = false;
    private bool actualReadyToRes = false;

    private int prevPlayerLives = 0;
    private int actualPlayerLives = 0;

    private int prevExtraLives = 0;
    private int actualExtraLives = 0;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnUpdate()
    {
        PlayerData player = GetSingleton<PlayerData>();

        // player res
        prevReadyToRes = actualReadyToRes;
        actualReadyToRes = player.readyToRes;
        if (actualReadyToRes && !prevReadyToRes)
        {
            MainGame.Instance.PlayerReadyToRes();
        }
        else if(prevReadyToRes && !actualReadyToRes)
        {
            MainGame.Instance.PlayerRes();
        }

        // extra life
        if (HasSingleton<ScoreData>())
        {
            ScoreData scoreData = GetSingleton<ScoreData>();
            prevExtraLives = actualExtraLives;
            actualExtraLives = scoreData.score / 2000;
            if (actualExtraLives != prevExtraLives)
            {
                player.lives += 1;
                SetSingleton<PlayerData>(player);
            }
        }

        // player lives
        prevPlayerLives = actualPlayerLives;
        actualPlayerLives = player.lives;
        if (actualPlayerLives != prevPlayerLives)
        {
            MainGame.Instance.SetPlayerLives(actualPlayerLives);
        }

        Entity gameStateEntity = GetSingletonEntity<GameStateRunning>();

        float dTime = Time.DeltaTime;
        bool wantsToRes = Input.anyKeyDown;

        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<DisabledTag>()
            .WithNone<HitTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref PlayerData player) =>
        {

            if (player.readyToRes && wantsToRes)
            {
                player.readyToRes = false;

                pw.RemoveComponent<DisabledTag>(entityInQueryIndex, entity);

                translation.Value = float3.zero;

                return;
            }

            player.resCooldownCounter = math.max(player.resCooldownCounter - dTime, 0f);

            if (player.resCooldownCounter <= 0f)
            {
                if (player.lives == 0)
                {
                    pw.AddComponent<GameStateGameOver>(entityInQueryIndex, gameStateEntity);
                }
                else
                {
                    player.readyToRes = true;
                }
            }

        }).Schedule();

        Entities
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
            player.readyToRes = false;

        }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
