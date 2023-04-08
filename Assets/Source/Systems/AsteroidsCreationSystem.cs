using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class AsteroidsCreationSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        Entity poolEntity = EntityManager.CreateEntity();
        EntityManager.AddComponent<AsteroidsPool>(poolEntity);
        DynamicBuffer<AsteroidReferenceBufferElement> buffer = EntityManager.AddBuffer<AsteroidReferenceBufferElement>(poolEntity);

        PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        for(int i=0; i<10; i++)
        {
            Entity asteroidEntity = EntityManager.Instantiate(entitiesPrefabs.asteroidEntityPrefab);
            AsteroidData asteroidData = EntityManager.GetComponentData<AsteroidData>(asteroidEntity);
            AsteroidReferenceBufferElement element = new AsteroidReferenceBufferElement();
            element.asteroid = asteroidEntity;
            buffer = EntityManager.GetBuffer<AsteroidReferenceBufferElement>(poolEntity);
            buffer.Add(element);
        }
    }

    protected override void OnUpdate()
    {
        
    }
}
