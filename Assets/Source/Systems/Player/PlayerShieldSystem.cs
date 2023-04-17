using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerShieldSystem : SystemBase
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
        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entity shieldPrefab = GetSingleton<PrefabsEntitiesReferences>().shieldEntityPrefab;

        float deltaTime = Time.DeltaTime;

        Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref PowerupShield shield, in Parent parent) =>
            {
                shield.timeCounter -= deltaTime;

                if(shield.timeCounter <= 0f)
                {
                    pw.RemoveComponent<PlayerShield>(entityInQueryIndex, parent.Value);
                    pw.DestroyEntity(entityInQueryIndex, entity);
                }

            }).Schedule();

        Entities
            .WithAll<PlayerData>()
            .WithAll<PowerupShieldHit>()
            .WithNone<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex) =>
            {                               
                Entity shieldEntity = pw.Instantiate(entityInQueryIndex, shieldPrefab);
                pw.AddComponent<Parent>(entityInQueryIndex, shieldEntity, new Parent() { Value = entity });
                pw.AddComponent<LocalToParent>(entityInQueryIndex, shieldEntity);
                pw.AddComponent<PowerupShield>(entityInQueryIndex, shieldEntity, new PowerupShield() { timeCounter = 10f });

                pw.RemoveComponent<PowerupShieldHit>(entityInQueryIndex, entity);
                pw.AddComponent<PlayerShield>(entityInQueryIndex, entity, new PlayerShield() { shieldRef = shieldEntity } );

                Entity fxEntity = pw.CreateEntity(entityInQueryIndex);
                pw.AddComponent<FXData>(entityInQueryIndex, fxEntity, new FXData() { fxId = FXEnum.AUDIO_POWERUP_PICKUP });

            }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
