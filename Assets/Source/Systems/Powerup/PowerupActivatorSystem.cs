using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PowerupActivatorSystem : SystemBase
{
    private EntityQuery powerupQuery;

    private bool prevAllDisabled;
    private bool currentAllDisabled;

    private float timeCounter;

    private bool running = false;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private double baseTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
    private Unity.Mathematics.Random randomM;


    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));

        EntityQueryDesc alienShipDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<PowerupDataCollectable>() }
        };
        powerupQuery = GetEntityQuery(alienShipDesc);
    }

    protected override void OnStartRunning()
    {
        prevAllDisabled = false;
        currentAllDisabled = false;

        uint randomSeed = (uint)(float)(baseTime + Time.ElapsedTime * 100);
        randomM.InitState(randomSeed);

        running = false;
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = beginSimulation_ecbs.CreateCommandBuffer();

        prevAllDisabled = currentAllDisabled;
        currentAllDisabled = powerupQuery.IsEmpty;

        if (currentAllDisabled && !prevAllDisabled)
        {
            timeCounter = randomM.NextFloat(0f, 1f);
            running = true;
        }
        else if (prevAllDisabled && !currentAllDisabled)
        {
        }

        if (running)
        {
            PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

            timeCounter -= Time.DeltaTime;

            if (timeCounter <= 0)
            {
                running = false;

                Entity entityPowerup = ecb.Instantiate(entitiesPrefabs.powerUpShieldEntityPrefab);

                float3 startPos = randomM.NextBool() ? Utils.GetRandomPosArea(ref randomM, 50f, 60f, 0f, 10f) : Utils.GetRandomPosArea(ref randomM, 0f, 15f, 50f, 60f);
                ecb.SetComponent<Translation>(entityPowerup, new Translation() { Value = startPos });

                float3 newDir = randomM.NextFloat3Direction();
                newDir.z = 0;
                newDir = math.normalize(newDir);

                ecb.SetComponent<PowerupDataCollectable>(entityPowerup, new PowerupDataCollectable() { direction = newDir, moveSpeed = 4f });
            }
        }
    }

    protected override void OnDestroy()
    {

    }

    protected override void OnStopRunning()
    {

    }
}
