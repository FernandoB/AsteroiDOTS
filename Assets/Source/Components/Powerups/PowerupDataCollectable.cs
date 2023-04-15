using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct PowerupDataCollectable : IComponentData
{
    public float moveSpeed;
    public float3 direction;
}