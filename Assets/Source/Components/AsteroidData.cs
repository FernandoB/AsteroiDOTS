using System;
using Unity.Entities;

[Serializable]
[GenerateAuthoringComponent]
public struct AsteroidData : IComponentData
{
    public float maxSpeed;
}
