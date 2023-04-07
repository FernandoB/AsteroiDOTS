using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PrefabsEntitiesReferences : IComponentData
{
    public Entity asteroidEntityPrefab;

    public int asteroidsCounter;
}
