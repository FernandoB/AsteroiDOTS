using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

public class CollideSystem : SystemBase
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
        TriggerJob triggerJob = new TriggerJob() {
            asteroidsData = GetComponentDataFromEntity<AsteroidData>(),
            bulletsData = GetComponentDataFromEntity<BulletData>(),
            entitiesToDelete = GetComponentDataFromEntity<DeleteTag>(),
            entitiesToBulletHit = GetComponentDataFromEntity<BulletHitTag>(),
            commandBuffer = endSimulation_ecbs.CreateCommandBuffer()
            };

        JobHandle jobHandle = triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);
        jobHandle.Complete();
    }

    private struct TriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<AsteroidData> asteroidsData;
        [ReadOnly] public ComponentDataFromEntity<BulletData> bulletsData;
        [ReadOnly] public ComponentDataFromEntity<DeleteTag> entitiesToDelete;
        [ReadOnly] public ComponentDataFromEntity<BulletHitTag> entitiesToBulletHit;
        public EntityCommandBuffer commandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            TestTriggers(triggerEvent.EntityA, triggerEvent.EntityB);
            TestTriggers(triggerEvent.EntityB, triggerEvent.EntityA);
        }

        private void TestTriggers(Entity entityA, Entity entityB)
        {
            if(asteroidsData.HasComponent(entityA)
                && bulletsData.HasComponent(entityB))
            {
                if( ! entitiesToBulletHit.HasComponent(entityA))
                {
                    commandBuffer.AddComponent<BulletHitTag>(entityA);
                }
                if( ! entitiesToDelete.HasComponent(entityB))
                {
                    commandBuffer.AddComponent<DeleteTag>(entityB);
                }
            }
        }
    }

}
