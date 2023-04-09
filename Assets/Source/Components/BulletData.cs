using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct BulletData : IComponentData
{
    public float maxSpeed;
    public float3 startVelocity;
}
