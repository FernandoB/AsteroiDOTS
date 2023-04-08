using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class AsteroidsUpdateStateSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        
    }

    protected override void OnUpdate()
    {
        EntityQuery asteroidsPoolQuery = GetEntityQuery(typeof(AsteroidsPool));
        NativeArray<Entity> asteroidsPool = asteroidsPoolQuery.ToEntityArray(Allocator.Temp);

        if(asteroidsPool.Length == 0)
        {
            return;
        }

        Entities.ForEach((ref DynamicBuffer<AsteroidReferenceBufferElement> buffer) =>
        {

            for(int i=0; i<buffer.Length; i++)
            {                
                AsteroidReferenceBufferElement element = buffer[i];
                AsteroidData asteroidData = GetComponent<AsteroidData>(element.asteroid);
                asteroidData.state++;
                SetComponent<AsteroidData>(element.asteroid, asteroidData);
            }

        }).Schedule();
    }
}
