using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct PlayerData : IComponentData
{
    public float size;
    public float acceleration;
    public float maxSpeed;
    public float currentSpeed;
    public float rotationSpeed;
    public float3 direction;
}
