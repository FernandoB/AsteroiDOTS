using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct BulletData : IComponentData
{
    public float lifeTime;
    public float maxSpeed;
    public float3 startVelocity;
    public float3 direction;
}
