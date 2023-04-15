using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct AlienShipData : IComponentData
{
    [ReadOnly]
    public float speed;
    [ReadOnly]
    public float size;

    public float3 direction;
    public float changeDirectionCounter;
    public float shootCounter;
}
