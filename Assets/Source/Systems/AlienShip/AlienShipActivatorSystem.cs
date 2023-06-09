﻿using Unity.Burst;
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

        uint randomSeed = (uint)(float)(baseTime + Time.ElapsedTime * 100);
        randomM.InitState(randomSeed);

        running = false;
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = beginSimulation_ecbs.CreateCommandBuffer();

        prevAllDisabled = currentAllDisabled;
        currentAllDisabled = alienShipQuery.IsEmpty;

        if(currentAllDisabled && !prevAllDisabled)
        {
            timeCounter = randomM.NextFloat(10f, 20f);
            running = true;
        }
        else if(prevAllDisabled && !currentAllDisabled)
        {
        }

        if(running)
        {
            timeCounter -= Time.DeltaTime;

            if(timeCounter <= 0)
            {
                running = false;

                bool select = randomM.NextBool();

                Entity alienShipEntity;
                AlienShipData alienShipData;

                if (select)
                {
                    alienShipEntity = alienShipBigEntity;
                    alienShipData = alienShipBigData;

                    Entity fxEntity = ecb.CreateEntity();
                    ecb.AddComponent<FXData>(fxEntity, new FXData() { fxId = FXEnum.AUDIO_LOOP_BIG });
                }
                else
                {
                    alienShipEntity = alienShipSmallEntity;
                    alienShipData = alienShipSmallData;

                    Entity fxEntity = ecb.CreateEntity();
                    ecb.AddComponent<FXData>(fxEntity, new FXData() { fxId = FXEnum.AUDIO_LOOP_SMALL });
                }

                randomM.InitState((uint)(float)(baseTime + Time.ElapsedTime * 100));
                alienShipData.changeDirectionCounter = randomM.NextFloat(2f, 5f);
                alienShipData.shootCounter = randomM.NextFloat(2f, 4f);
                float3 newDir = randomM.NextFloat3Direction() + alienShipData.direction;
                newDir.z = 0;
                newDir = math.normalize(newDir);
                alienShipData.direction = newDir;
                float3 startPos = randomM.NextBool() ? Utils.GetRandomPosArea(ref randomM, 50f, 60f, 0f, 10f) : Utils.GetRandomPosArea(ref randomM, 0f, 15f, 50f, 60f);

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
}
