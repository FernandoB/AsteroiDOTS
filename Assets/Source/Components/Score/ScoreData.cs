using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct ScoreData : IComponentData
{
    public int score;
}