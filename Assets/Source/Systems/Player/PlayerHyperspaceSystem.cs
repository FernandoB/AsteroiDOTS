using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerHyperspaceSystem : SystemBase
{
    private const float outOfThisWorld = 300f;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private double baseTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
    private Unity.Mathematics.Random randomM;

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
        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Unity.Mathematics.Random random = randomM;
        double elapsedTime = Time.ElapsedTime;
        double bTime = baseTime;

        float deltaTime = Time.DeltaTime;

        bool hyperspace = false;
        if(Input.GetKeyDown(KeyCode.H))
        {
            hyperspace = true;
        }

        if (hyperspace)
        {
            Entities
                .WithAll<PlayerData>()
                .WithNone<PlayerHyperspace>()
                .WithNone<DisabledTag>()
                .ForEach((Entity entity, int entityInQueryIndex, ref PlayerData player, ref Translation translation, ref Rotation rotation) =>
                {
                    pw.AddComponent<PlayerHyperspace>(entityInQueryIndex, entity, new PlayerHyperspace() { timeCounter = 1f });
                    pw.AddComponent<DisabledTag>(entityInQueryIndex, entity, new DisabledTag());

                    Entity fxExpEntity = pw.CreateEntity(entityInQueryIndex);
                    pw.AddComponent<FXData>(entityInQueryIndex, fxExpEntity, new FXData() { fxId = FXEnum.EXPLOSION, posX = translation.Value.x, posY = translation.Value.y });

                    Entity fxEntity = pw.CreateEntity(entityInQueryIndex);
                    pw.AddComponent<FXData>(entityInQueryIndex, fxEntity, new FXData() { fxId = FXEnum.AUDIO_HYPERSPACE });

                    translation.Value = new float3(-outOfThisWorld, outOfThisWorld, 0);
                    rotation.Value = quaternion.identity;
                    player.currentSpeed = 0f;
                    player.direction = float3.zero;

                }).Schedule();
        }

        Entities
            .WithAll<PlayerData>()
            .WithAll<PlayerHyperspace>()
            .WithAll<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref PlayerHyperspace hyper, ref PlayerData player, ref Translation translation, ref Rotation rotation) =>
            {
                hyper.timeCounter -= deltaTime;

                if (hyper.timeCounter < 0)
                {
                    uint randomSeed = (uint)(float)(bTime + elapsedTime * 100);
                    random.InitState(randomSeed);

                    pw.RemoveComponent<PlayerHyperspace>(entityInQueryIndex, entity);
                    pw.RemoveComponent<DisabledTag>(entityInQueryIndex, entity);

                    if(random.NextFloat(0f, 6f) < 1f)
                    {
                        pw.AddComponent<HitTag>(entityInQueryIndex, entity);

                        float3 newPosHit = Utils.GetRandomPosArea(ref random, 0f, 17.5f, 0f, 12f);

                        translation.Value = newPosHit;
                        rotation.Value = quaternion.identity;
                        player.currentSpeed = 0f;
                        player.direction = float3.zero;

                        return;
                    }

                    float3 newPos = Utils.GetRandomPosArea(ref random, 0f, 17.5f, 0f, 12f);

                    Entity fxExpEntity = pw.CreateEntity(entityInQueryIndex);
                    pw.AddComponent<FXData>(entityInQueryIndex, fxExpEntity, new FXData() { fxId = FXEnum.EXPLOSION, posX = newPos.x, posY = newPos.y });

                    Entity fxEntity = pw.CreateEntity(entityInQueryIndex);
                    pw.AddComponent<FXData>(entityInQueryIndex, fxEntity, new FXData() { fxId = FXEnum.AUDIO_HYPERSPACE });

                    translation.Value = newPos;
                    rotation.Value = quaternion.identity;
                    player.currentSpeed = 0f;
                    player.direction = float3.zero;
                }

            }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
