using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct AlienShipData : IComponentData
{
    public float maxSpeed;
    public float3 direction;
}
