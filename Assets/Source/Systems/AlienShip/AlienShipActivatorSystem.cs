using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AlienShipActivatorSystem : SystemBase
{
    private Entity alienShipBigEntity;
    private AlienShipData alienShipBigData;

    private Entity alienShipSmallEntity;
    private AlienShipData alienShipSmallData;

    private EntityQuery alienShipQuery;

    private bool prevAllDisabled;
    private bool currentAllDisabled;

    private float timeCounter;

    private bool running = false;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;


    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));

        EntityQueryDesc alienShipDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { ComponentType.ReadOnly<DisabledTag>() },
            All = new ComponentType[] { ComponentType.ReadOnly<AlienShipData>() }
        };
        alienShipQuery = GetEntityQuery(alienShipDesc);
    }

    protected override void OnStartRunning()
    {
        prevAllDisabled = false;
        currentAllDisabled = false;

        alienShipBigEntity = GetSingletonEntity<AlienShipBigTag>();
        alienShipBigData = EntityManager.GetComponentData<AlienShipData>(alienShipBigEntity);
        alienShipSmallEntity = GetSingletonEntity<AlienShipSmallTag>();
        alienShipSmallData = EntityManager.GetComponentData<AlienShipData>(alienShipSmallEntity);

        timeCounter = 2f;
        running = false;
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = beginSimulation_ecbs.CreateCommandBuffer();

        prevAllDisabled = currentAllDisabled;
        currentAllDisabled = alienShipQuery.IsEmpty;

        if(currentAllDisabled && !prevAllDisabled)
        {
            Debug.Log("START RUNNING");
            timeCounter = 2f;
            running = true;
        }
        else if(prevAllDisabled && !currentAllDisabled)
        {
            Debug.Log("ONE ENABLED");
        }

        if(running)
        {
            timeCounter -= Time.DeltaTime;

            if(timeCounter <= 0)
            {
                Debug.Log("SPAWN");
                running = false;

                bool select = false;

                Entity alienShipEntity;
                AlienShipData alienShipData;

                if (select)
                {
                    alienShipEntity = alienShipBigEntity;
                    alienShipData = alienShipBigData;
                }
                else
                {
                    alienShipEntity = alienShipSmallEntity;
                    alienShipData = alienShipSmallData;
                }

                alienShipData.direction = new float3(1f, 1f, 0f);
                ecb.SetComponent<AlienShipData>(alienShipEntity, alienShipData);
                ecb.RemoveComponent<DisabledTag>(alienShipEntity);
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
