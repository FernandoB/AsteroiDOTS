using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class WeaponASystem : SystemBase
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
            .WithAll<PlayerData>()
            .WithNone<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref PowerUpWeaponA weapon) =>
            {
                weapon.timeCounter -= deltaTime;

                if (weapon.timeCounter <= 0f)
                {
                    pw.RemoveComponent<PowerUpWeaponA>(entityInQueryIndex, entity);
                }

            }).Schedule();

        Entities
            .WithAll<PlayerData>()
            .WithAll<PowerupWeaponAHit>()
            .WithNone<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex) =>
            {
                pw.RemoveComponent<PowerupWeaponAHit>(entityInQueryIndex, entity);
                pw.AddComponent<PowerUpWeaponA>(entityInQueryIndex, entity, new PowerUpWeaponA() { timeCounter = 10f });

            }).Schedule();

        Entities
            .WithAll<PlayerData>()
            .WithAll<PowerUpWeaponA>()
            .WithAll<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex) =>
            {
                pw.RemoveComponent<PowerUpWeaponA>(entityInQueryIndex, entity);                

            }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
