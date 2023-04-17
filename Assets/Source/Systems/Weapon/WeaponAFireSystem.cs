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

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));

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
            .ForEach((Entity entity, int entityInQueryIndex, in PlayerData player, in Translation translation, in Rotation rotation, in LocalToWorld local2World) =>
            {

                Entity bullet = pw.Instantiate(entityInQueryIndex, prefab);

                Rotation bulletRotation = new Rotation();
                bulletRotation.Value = quaternion.LookRotation(local2World.Up, new float3(0f, 0f, 1f));

                Translation bulletTranslation = new Translation();
                bulletTranslation.Value = translation.Value + (local2World.Up * player.size);

                BulletData bulletData = new BulletData();
                bulletData.maxSpeed = bulletMaxSpeed;
                bulletData.lifeTime = lifeTime;
                bulletData.startVelocity = math.project(player.direction, local2World.Up);
                bulletData.direction = local2World.Up;

                pw.SetComponent<Translation>(entityInQueryIndex, bullet, bulletTranslation);
                pw.SetComponent<Rotation>(entityInQueryIndex, bullet, bulletRotation);
                pw.SetComponent<BulletData>(entityInQueryIndex, bullet, bulletData);
                pw.AddComponent<PlayerBullet>(entityInQueryIndex, bullet);

                // right
                bullet = pw.Instantiate(entityInQueryIndex, prefab);

                bulletRotation = new Rotation();
                bulletRotation.Value = quaternion.LookRotation(local2World.Up, new float3(0f, 0f, 1f));

                bulletTranslation = new Translation();
                bulletTranslation.Value = translation.Value + (local2World.Right * 0.75f);

                bulletData = new BulletData();
                bulletData.maxSpeed = bulletMaxSpeed;
                bulletData.lifeTime = lifeTime;
                bulletData.startVelocity = math.project(player.direction, local2World.Up);
                bulletData.direction = local2World.Up;

                pw.SetComponent<Translation>(entityInQueryIndex, bullet, bulletTranslation);
                pw.SetComponent<Rotation>(entityInQueryIndex, bullet, bulletRotation);
                pw.SetComponent<BulletData>(entityInQueryIndex, bullet, bulletData);
                pw.AddComponent<PlayerBullet>(entityInQueryIndex, bullet);

                // left
                bullet = pw.Instantiate(entityInQueryIndex, prefab);

                bulletRotation = new Rotation();
                bulletRotation.Value = quaternion.LookRotation(local2World.Up, new float3(0f, 0f, 1f));

                bulletTranslation = new Translation();
                bulletTranslation.Value = translation.Value - (local2World.Right * 0.75f);

                bulletData = new BulletData();
                bulletData.maxSpeed = bulletMaxSpeed;
                bulletData.lifeTime = lifeTime;
                bulletData.startVelocity = math.project(player.direction, local2World.Up);
                bulletData.direction = local2World.Up;

                pw.SetComponent<Translation>(entityInQueryIndex, bullet, bulletTranslation);
                pw.SetComponent<Rotation>(entityInQueryIndex, bullet, bulletRotation);
                pw.SetComponent<BulletData>(entityInQueryIndex, bullet, bulletData);
                pw.AddComponent<PlayerBullet>(entityInQueryIndex, bullet);

                Entity fxEntity = pw.CreateEntity(entityInQueryIndex);
                pw.AddComponent<FXData>(entityInQueryIndex, fxEntity, new FXData() { fxId = FXEnum.AUDIO_FIRE });

            }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
