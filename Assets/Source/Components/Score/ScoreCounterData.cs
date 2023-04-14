using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct ScoreCounterData : IComponentData
{
    public int score;
    public int scoreCount;
}