using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct AsteroidDivision : IComponentData
{
    public float3 position;
}