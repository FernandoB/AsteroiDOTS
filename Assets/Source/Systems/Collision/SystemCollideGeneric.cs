using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

public class SystemCollideGeneric<T1_A, T2_A, T3_B, T4_B> : SystemBase where T1_A : struct, IComponentData where T2_A : struct, IComponentData where T3_B : struct, IComponentData where T4_B : struct, IComponentData
{
    private BeginSimulationEntityCommandBufferSystem simulation_ecbs;
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        base.OnCreate();

        simulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = simulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        TriggerJobGeneric<T1_A, T2_A, T3_B, T4_B> triggerJob = new TriggerJobGeneric<T1_A, T2_A, T3_B, T4_B>()
        {
            toCheck_T1A = GetComponentDataFromEntity<T1_A>(),
            toAdd_T2A = GetComponentDataFromEntity<T2_A>(),
            toCheck_T3B = GetComponentDataFromEntity<T3_B>(),
            toAdd_T4B = GetComponentDataFromEntity<T4_B>(),
            commandBuffer = ecb
        };

        Dependency = triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        simulation_ecbs.AddJobHandleForProducer(Dependency);
    }
}
