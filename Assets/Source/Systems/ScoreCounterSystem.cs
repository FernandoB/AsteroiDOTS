using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[UpdateAfter(typeof(AsteroidsUpdateStateSystem))]
public class ScoreCounterSystem : SystemBase
{
    private EntityQuery scoreDataQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        scoreDataQuery = GetEntityQuery(ComponentType.ReadOnly<ScoreCounterData>());
    }

    protected override void OnUpdate()
    {
        NativeArray<ScoreCounterData> scores = scoreDataQuery.ToComponentDataArray<ScoreCounterData>(Allocator.Temp);

        int scoreAcum = 0;

        for(int i=0; i<scores.Length; i++)
        {
            scoreAcum += scores[i].scoreCount;
        }

        //Debug.Log("score: " + scoreAcum);

        scores.Dispose();
    }
}
