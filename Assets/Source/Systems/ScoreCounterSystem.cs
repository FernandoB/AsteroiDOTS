using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

[UpdateAfter(typeof(AsteroidsUpdateStateSystem))]
public class ScoreCounterSystem : SystemBase
{
    private EntityQuery scoreQuery;

    private int prevScore = 0;
    private int actualScore = 0;

    private JobHandle job;

    private NativeArray<int> scoreAcum;

    private bool jobRunning = false;

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));

        scoreQuery = GetEntityQuery(ComponentType.ReadOnly<AsteroidData>(), ComponentType.ReadOnly<ScoreCounterData>());
    }

    protected override void OnStartRunning()
    {
        prevScore = -1;
        actualScore = 0;

        scoreAcum = new NativeArray<int>(1, Allocator.Persistent);
    }

    protected override void OnUpdate()
    {                
        if (jobRunning)
        {
            if (job.IsCompleted)
            {
                jobRunning = false;
                job.Complete();

                prevScore = actualScore;
                actualScore = scoreAcum[0];
                if (actualScore != prevScore)
                {
                    MainGame.Instance.SetScore(actualScore);
                }
            }
        }
        else if( ! jobRunning)
        {
            jobRunning = true;

            NativeArray<ScoreCounterData> scoreDatas = scoreQuery.ToComponentDataArray<ScoreCounterData>(Allocator.TempJob);

            scoreAcum[0] = 0;
            NativeArray<int> scoreArr = scoreAcum;

            job = Job.WithCode(() =>
            {
                for (int i = 0; i < scoreDatas.Length; i++)
                {
                    scoreArr[0] += scoreDatas[i].scoreCount * scoreDatas[i].score;
                }
            }).Schedule(Dependency);

            scoreDatas.Dispose(job);
        }
    }

    protected override void OnStopRunning()
    {
        job.Complete();
        scoreAcum.Dispose();
    }
}
