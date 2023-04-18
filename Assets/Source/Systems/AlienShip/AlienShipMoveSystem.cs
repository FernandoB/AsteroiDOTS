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

    private PrefabsEntitiesReferences entitiesPrefabs;
    private Entity bulletPrefab;
    private BulletData bulletDataPrefab;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnStartRunning()
    {
        entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        bulletPrefab = entitiesPrefabs.bulletEntityPrefab;

        bulletDataPrefab = GetComponent<BulletData>(bulletPrefab);
    }

    protected override void OnUpdate()
    {        
        EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        Entity playerEntity = GetSingletonEntity<PlayerData>();
        Translation playerPosition = EntityManager.GetComponentData<Translation>(playerEntity);
        bool playerActive = !EntityManager.HasComponent<DisabledTag>(playerEntity);

        Unity.Mathematics.Random random = randomM;
        float deltaTime = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;
        double bTime = baseTime;

        Entity bulletCapPrefab = bulletPrefab;
        float bulletMaxSpeed = bulletDataPrefab.maxSpeed;
        float lifeTime = bulletDataPrefab.lifeTime;

        Entities
            .WithNone<DisabledTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, ref AlienShipData alienShipData) =>
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

                alienShipData.shootCounter -= deltaTime;
                if(alienShipData.shootCounter <= 0)
                {
                    alienShipData.shootCounter = random.NextFloat(2f, 4f);

                    Entity bullet = pw.Instantiate(entityInQueryIndex, bulletCapPrefab);

                    float3 deltaTargetPos = new float3(random.NextFloat(0f, 1f), random.NextFloat(0f, 1f), 0f);
                    deltaTargetPos.x *= random.NextBool() ? -1f : 1f;
                    deltaTargetPos.y *= random.NextBool() ? -1f : 1f;
                    deltaTargetPos = math.normalize(deltaTargetPos) * random.NextFloat(3f, 6f);

                    float3 deltaVec;
                    if (playerActive)
                    {
                        deltaVec = (playerPosition.Value + deltaTargetPos) - translation.Value;
                    }
                    else
                    {
                        deltaVec = (translation.Value + deltaTargetPos) - translation.Value;
                    }
                    deltaVec.z = 0;
                    deltaVec = math.normalize(deltaVec);

                    Rotation bulletRotation = new Rotation();
                    bulletRotation.Value = quaternion.LookRotation(deltaVec, new float3(0f, 0f, 1f));

                    Translation bulletTranslation = new Translation();
                    bulletTranslation.Value = translation.Value + (deltaVec * alienShipData.size);

                    BulletData bulletData = new BulletData();
                    bulletData.maxSpeed = bulletMaxSpeed;
                    bulletData.lifeTime = lifeTime;
                    bulletData.startVelocity = float3.zero;
                    bulletData.direction = deltaVec;

                    pw.SetComponent<Translation>(entityInQueryIndex, bullet, bulletTranslation);
                    pw.SetComponent<Rotation>(entityInQueryIndex, bullet, bulletRotation);
                    pw.SetComponent<BulletData>(entityInQueryIndex, bullet, bulletData);
                    pw.AddComponent<AlienShipBullet>(entityInQueryIndex, bullet);

                    Entity fxEntity = pw.CreateEntity(entityInQueryIndex);
                    pw.AddComponent<FXData>(entityInQueryIndex, fxEntity, new FXData() { fxId = FXEnum.AUDIO_FIRE });
                }

                translation.Value = translation.Value + (alienShipData.direction * alienShipData.speed * deltaTime);
                rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(math.radians(alienShipData.rotationSpeed * deltaTime)));

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

                Entity fxEntity = pw.CreateEntity(entityInQueryIndex);
                pw.AddComponent<FXData>(entityInQueryIndex, fxEntity, new FXData() { fxId = FXEnum.AUDIO_ASTEROID_BIG });

                Entity loopStopEntity = pw.CreateEntity(entityInQueryIndex);
                pw.AddComponent<FXData>(entityInQueryIndex, loopStopEntity, new FXData() { fxId = FXEnum.AUDIO_STOP_LOOP });

            }).ScheduleParallel();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
