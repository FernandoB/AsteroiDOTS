using System;
using Unity.Entities;

[InternalBufferCapacity(10)]
public struct AsteroidReferenceBufferElement : IBufferElementData
{
    public Entity asteroid;

    public static implicit operator AsteroidReferenceBufferElement(Entity value)
    {
        return new AsteroidReferenceBufferElement() { asteroid = value };
    }

    public static implicit operator Entity(AsteroidReferenceBufferElement element)
    {
        return element.asteroid;
    }
}
