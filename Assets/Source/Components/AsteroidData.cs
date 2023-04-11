using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct AsteroidData : IComponentData
{
    public Entity entity;
    public float speed;
    public float3 direction;
}
