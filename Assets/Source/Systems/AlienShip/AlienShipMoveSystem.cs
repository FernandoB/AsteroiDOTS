using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AlienShipMoveSystem : SystemBase
{
    private double baseTime = System.DateTime.Now.TimeOfDay.TotalSeconds;

    private const float outOfThisWorld = 300f;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private Unity.Mathematics.Random randomM;


    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnUpdate()
    {        
        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Unity.Mathematics.Random random = randomM;
        float deltaTime = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;
        double bTime = baseTime;

        Entities
            .WithNone<DisabledTag>()
            .ForEach((ref Translation translation, ref AlienShipData alienShipData) =>
            {
                uint randomSeed = (uint)(float)(bTime + elapsedTime * 100);
                random.InitState(randomSeed);

                alienShipData.changeDirectionCounter -= deltaTime;
                if(alienShipData.changeDirectionCounter <= 0f)
                {
                    alienShipData.changeDirectionCounter = random.NextFloat(2f, 5f);
                    float3 newDir = random.NextFloat3Direction() + alienShipData.direction;
                    newDir.z = 0;
                    newDir = math.normalize(newDir);
                    alienShipData.direction = newDir;
                }

                translation.Value = translation.Value + (alienShipData.direction * alienShipData.speed * deltaTime);

            }).ScheduleParallel();

        Entities
            .WithNone<DisabledTag>()
            .WithAll<HitTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref ScoreCounterData scoreCounter, in AlienShipData alienShipData) =>
            {
                pw.AddComponent<DisabledTag>(entityInQueryIndex, entity);
                pw.RemoveComponent<HitTag>(entityInQueryIndex, entity);

                float3 t = translation.Value;
                t.x = -outOfThisWorld;
                t.y = -outOfThisWorld;
                translation.Value = t;

                scoreCounter.scoreCount = scoreCounter.scoreCount + 1;

            }).ScheduleParallel();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
