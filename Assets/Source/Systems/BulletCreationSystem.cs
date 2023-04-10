using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BulletCreationSystem : SystemBase
{
    private Entity bulletPrefab;

    private BulletData bulletDataPrefab;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
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

        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entity prefab = bulletPrefab;

        float bulletMaxSpeed = bulletDataPrefab.maxSpeed;

        Entities.ForEach((Entity entity, int entityInQueryIndex, in PlayerData player, in Translation translation, in Rotation rotation, in LocalToWorld local2World) =>
        {
            
            Entity bullet = pw.Instantiate(entityInQueryIndex, prefab);       

            Rotation bulletRotation = new Rotation();
            bulletRotation.Value = rotation.Value;

            Translation bulletTranslation = new Translation();
            bulletTranslation.Value = translation.Value + (local2World.Up * player.size);

            BulletData bulletData = new BulletData();
            bulletData.maxSpeed = bulletMaxSpeed;
            bulletData.startVelocity = math.project(player.direction, local2World.Up);

            pw.SetComponent<Rotation>(entityInQueryIndex, bullet, bulletRotation);
            pw.SetComponent<Translation>(entityInQueryIndex, bullet, bulletTranslation);
            pw.SetComponent<BulletData>(entityInQueryIndex, bullet, bulletData);

        }).Schedule();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
