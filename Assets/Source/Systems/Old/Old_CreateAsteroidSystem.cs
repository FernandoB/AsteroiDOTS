using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class Old_CreateAsteroidSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    protected override void OnCreate()
    {
        //base.OnCreate();

        //beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //EntityCommandBuffer.ParallelWriter pw = beginSimulation_ecbs.CreateCommandBuffer().AsParallelWriter();

        //Entities
        //    .WithAll<PrefabsEntitiesReferences>()
        //    .ForEach((Entity entity, int entityInQueryIndex, ref PrefabsEntitiesReferences prefabs) =>
        //    {
        //        if(prefabs.asteroidsCounter > 0)
        //        {
        //            return;
        //        }

        //        pw.Instantiate(entityInQueryIndex, prefabs.asteroidEntityPrefab);

        //        prefabs.asteroidsCounter++;
        //    }
        //    ).Schedule();

        //beginSimulation_ecbs.AddJobHandleForProducer(this.Dependency);
    }
}
