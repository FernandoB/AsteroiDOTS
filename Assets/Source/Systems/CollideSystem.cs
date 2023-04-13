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
        EntityCommandBuffer.ParallelWriter ecb = endSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        TriggerJob triggerJob = new TriggerJob() {
            asteroidsData = GetComponentDataFromEntity<AsteroidData>(),
            bulletsData = GetComponentDataFromEntity<BulletData>(),
            entitiesToDelete = GetComponentDataFromEntity<DeleteTag>(),
            entitiesToBulletHit = GetComponentDataFromEntity<BulletHitTag>(),
            commandBuffer = ecb
        };

        Dependency = triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        endSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    private struct TriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<AsteroidData> asteroidsData;
        [ReadOnly] public ComponentDataFromEntity<BulletData> bulletsData;
        [ReadOnly] public ComponentDataFromEntity<DeleteTag> entitiesToDelete;
        [ReadOnly] public ComponentDataFromEntity<BulletHitTag> entitiesToBulletHit;
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            TestTriggers(triggerEvent.EntityA, triggerEvent.BodyIndexA, triggerEvent.EntityB, triggerEvent.BodyIndexB);
            TestTriggers(triggerEvent.EntityB, triggerEvent.BodyIndexA, triggerEvent.EntityA, triggerEvent.BodyIndexB);
        }

        private void TestTriggers(Entity entityA, int indexA, Entity entityB, int indexB)
        {
            if(asteroidsData.HasComponent(entityA)
                && bulletsData.HasComponent(entityB))
            {
                if( ! entitiesToBulletHit.HasComponent(entityA))
                {
                    commandBuffer.AddComponent<BulletHitTag>(indexA, entityA);
                }
                if( ! entitiesToDelete.HasComponent(entityB))
                {
                    commandBuffer.AddComponent<DeleteTag>(indexB, entityB);
                }
            }
        }
    }

}
