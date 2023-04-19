using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class WeaponAFireSystem : SystemBase
{
    private Entity bulletPrefab;

    private BulletData bulletDataPrefab;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private EntityQuery bulletsQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate(GetEntityQuery(typeof(PowerUpWeaponA)));

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        bulletsQuery = GetEntityQuery(ComponentType.ReadOnly<BulletData>(), ComponentType.ReadOnly<PlayerBullet>());
    }

    protected override void OnStartRunning()
    {
        PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        bulletPrefab = entitiesPrefabs.bulletEntityPrefab;

        bulletDataPrefab = GetComponent<BulletData>(bulletPrefab);
    }

    protected override void OnUpdate()
    {
        bool fire = false;
        fire = Input.GetKeyDown(KeyCode.Space);
        if (!fire) return;

        int count = bulletsQuery.CalculateEntityCount();
        if (count >= 12) return;

        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entity prefab = bulletPrefab;

        float bulletMaxSpeed = bulletDataPrefab.maxSpeed;
        float lifeTime = bulletDataPrefab.lifeTime;

        Entities
            .WithAll<PowerUpWeaponA>()
            .WithNone<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex, in PlayerData player, in Translation translation, in LocalToWorld local2World) =>
            {

                SetBullet(ref prefab, bulletMaxSpeed, lifeTime, local2World.Up * player.size, ref pw, entityInQueryIndex, in player, in translation, in local2World);

                SetBullet(ref prefab, bulletMaxSpeed, lifeTime, local2World.Right * 0.70f, ref pw, entityInQueryIndex, in player, in translation, in local2World);

                SetBullet(ref prefab, bulletMaxSpeed, lifeTime, local2World.Right * -0.70f, ref pw, entityInQueryIndex, in player, in translation, in local2World);

                Entity fxEntity = pw.CreateEntity(entityInQueryIndex);
                pw.AddComponent<FXData>(entityInQueryIndex, fxEntity, new FXData() { fxId = FXEnum.AUDIO_FIRE });

            }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    private static void SetBullet(  ref Entity prefab,
                                    float bulletMaxSpeed,
                                    float lifeTime,
                                    float3 posDelta,
                                    ref EntityCommandBuffer.ParallelWriter pw,
                                    int index,                                    
                                    in PlayerData player,
                                    in Translation translation,
                                    in LocalToWorld local2World)
    {
        Entity bullet = pw.Instantiate(index, prefab);

        Rotation bulletRotation = new Rotation();
        bulletRotation.Value = quaternion.LookRotation(local2World.Up, new float3(0f, 0f, 1f));

        Translation bulletTranslation = new Translation();
        bulletTranslation.Value = translation.Value + posDelta;

        BulletData bulletData = new BulletData();
        bulletData.maxSpeed = bulletMaxSpeed;
        bulletData.lifeTime = lifeTime;
        bulletData.startVelocity = math.project(player.direction, local2World.Up);
        bulletData.direction = local2World.Up;

        pw.SetComponent<Translation>(index, bullet, bulletTranslation);
        pw.SetComponent<Rotation>(index, bullet, bulletRotation);
        pw.SetComponent<BulletData>(index, bullet, bulletData);
        pw.AddComponent<PlayerBullet>(index, bullet);
    }
}
