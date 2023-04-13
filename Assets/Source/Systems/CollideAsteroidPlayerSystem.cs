﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

public class CollideAsteroidPlayerSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulation_ecbs;
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        base.OnCreate();

        endSimulation_ecbs = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = endSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        TriggerJob triggerJob = new TriggerJob()
        {
            asteroidsData = GetComponentDataFromEntity<AsteroidData>(),
            playerData = GetComponentDataFromEntity<PlayerData>(),
            entitiesToHit = GetComponentDataFromEntity<HitTag>(),
            commandBuffer = ecb
        };

        Dependency = triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        endSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    private struct TriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<AsteroidData> asteroidsData;
        [ReadOnly] public ComponentDataFromEntity<PlayerData> playerData;
        [ReadOnly] public ComponentDataFromEntity<HitTag> entitiesToHit;
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            TestTriggers(triggerEvent.EntityA, triggerEvent.BodyIndexA, triggerEvent.EntityB, triggerEvent.BodyIndexB);
            TestTriggers(triggerEvent.EntityB, triggerEvent.BodyIndexA, triggerEvent.EntityA, triggerEvent.BodyIndexB);
        }

        private void TestTriggers(Entity entityA, int indexA, Entity entityB, int indexB)
        {
            if (asteroidsData.HasComponent(entityA)
                && playerData.HasComponent(entityB))
            {
                if (!entitiesToHit.HasComponent(entityA))
                {
                    commandBuffer.AddComponent<HitTag>(indexA, entityA);
                }
                if (!entitiesToHit.HasComponent(entityB))
                {
                    commandBuffer.AddComponent<HitTag>(indexB, entityB);
                }
            }
        }
    }

}
