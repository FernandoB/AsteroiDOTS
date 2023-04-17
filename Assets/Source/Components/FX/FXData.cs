using System;
using Unity.Entities;
using Unity.Mathematics;

public enum FXEnum
{
    EXPLOSION,
    AUDIO_FIRE,
    AUDIO_ASTEROID_BIG,
    AUDIO_ASTEROID_MEDIUM,
    AUDIO_ASTEROID_SMALL,
    AUDIO_POWERUP_PICKUP,
    AUDIO_EXTRA_LIFE,
    AUDIO_STOP_LOOP,
    AUDIO_LOOP_BIG,
    AUDIO_LOOP_SMALL,
    AUDIO_HYPERSPACE
}

public struct FXData : IComponentData
{
    public FXEnum fxId;
    public float posX;
    public float posY;
}