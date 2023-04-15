
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

public struct TriggerJobGeneric<T1_A, T2_A, T3_B, T4_B> : ITriggerEventsJob where T1_A : struct, IComponentData where T2_A : struct, IComponentData where T3_B : struct, IComponentData where T4_B : struct, IComponentData
{
    [ReadOnly] public ComponentDataFromEntity<T1_A> toCheck_T1A;
    [ReadOnly] public ComponentDataFromEntity<T2_A> toAdd_T2A;

    [ReadOnly] public ComponentDataFromEntity<T3_B> toCheck_T3B;
    [ReadOnly] public ComponentDataFromEntity<T4_B> toAdd_T4B;

    public EntityCommandBuffer.ParallelWriter commandBuffer;

    public void Execute(TriggerEvent triggerEvent)
    {
        TestTriggers(triggerEvent.EntityA, triggerEvent.BodyIndexA, triggerEvent.EntityB, triggerEvent.BodyIndexB);
        TestTriggers(triggerEvent.EntityB, triggerEvent.BodyIndexB, triggerEvent.EntityA, triggerEvent.BodyIndexA);
    }

    private void TestTriggers(Entity entityA, int indexA, Entity entityB, int indexB)
    {
        if (toCheck_T1A.HasComponent(entityA)
            && toCheck_T3B.HasComponent(entityB))
        {
            if (!toAdd_T2A.HasComponent(entityA))
            {
                commandBuffer.AddComponent<T2_A>(indexA, entityA);
            }
            if (!toAdd_T4B.HasComponent(entityB))
            {
                commandBuffer.AddComponent<T4_B>(indexB, entityB);
            }
        }
    }
}