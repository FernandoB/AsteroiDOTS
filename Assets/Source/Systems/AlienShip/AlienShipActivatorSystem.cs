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

    private double baseTime = System.DateTime.Now.TimeOfDay.TotalSeconds;
    private Unity.Mathematics.Random randomM;


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

        uint randomSeed = (uint)(float)(baseTime + Time.ElapsedTime * 100);
        randomM.InitState(randomSeed);
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

                randomM.InitState((uint)(float)(baseTime + Time.ElapsedTime * 100));
                alienShipData.changeDirectionCounter = randomM.NextFloat(2f, 5f);
                alienShipData.shootCounter = 1f;
                float3 newDir = randomM.NextFloat3Direction() + alienShipData.direction;
                newDir.z = 0;
                newDir = math.normalize(newDir);
                alienShipData.direction = newDir;
                float3 startPos = randomM.NextBool() ? GetRandomPosArea(ref randomM, 50f, 60f, 0f, 10f) : GetRandomPosArea(ref randomM, 0f, 15f, 50f, 60f);

                ecb.SetComponent<Translation>(alienShipEntity, new Translation() { Value = startPos } );
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

    private static float3 GetRandomPosArea(ref Unity.Mathematics.Random randon, float minX, float maxX, float minY, float maxY)
    {
        return new float3(randon.NextFloat(minX, maxX) * (randon.NextBool() ? -1f : 1f),
                            randon.NextFloat(minY, maxY) * (randon.NextBool() ? -1f : 1f),
                            0f);
    }
}
