using System;
using Unity.Entities;

[InternalBufferCapacity(10)]
public struct AsteroidReferenceBufferElement : IBufferElementData
{
    public AsteroidData asteroid;
}
