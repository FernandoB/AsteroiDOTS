using System;
using Unity.Entities;
using Unity.Mathematics;

public enum FXEnum
{
    EXPLOSION,
    AUDIO
}

public struct FXData : IComponentData
{
    public FXEnum fxId;
    public float posX;
    public float posY;
}