using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct PlayerData : IComponentData
{
    // config data
    public float size;
    public float acceleration;
    public float maxSpeed;    
    public float rotationSpeed;
    public float resCooldown;
    public int lives;

    // update data
    public float resCooldownCounter;
    public float currentSpeed;
    public float3 direction;
    public bool readyToRes;
}
