using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct AsteroidData : IComponentData
{
    public float speed;
    public float3 direction;
    public Unity.Mathematics.Random random;
}
