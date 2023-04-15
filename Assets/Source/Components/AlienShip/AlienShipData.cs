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

    public float3 direction;
    public float changeDirectionCounter;
}
