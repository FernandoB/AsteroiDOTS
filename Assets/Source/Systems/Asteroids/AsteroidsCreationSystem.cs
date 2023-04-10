using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsCreationSystem : SystemBase
{
    private Entity asteroidPrefab;

    private AsteroidData asteroidDataPrefab;

    private EntityQuery asteroidsQuery;

    private BeginSimulationEntityCommandBufferSystem beginSimulation_ecbs;

    private Unity.Mathematics.Random random;

    private float2[] rPos;
    NativeArray<float2> nRPos;

    private const int amountAsteroids = 3;

    protected override void OnCreate()
    {
        base.OnCreate();

        beginSimulation_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        PrefabsEntitiesReferences entitiesPrefabs = GetSingleton<PrefabsEntitiesReferences>();

        asteroidPrefab = entitiesPrefabs.asteroidBigEntityPrefab;

        asteroidDataPrefab = GetComponent<AsteroidData>(asteroidPrefab);

        asteroidsQuery = GetEntityQuery(typeof(AsteroidData));

        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        uint cur_time = (uint)(System.DateTime.UtcNow - epochStart).TotalSeconds;

        random = new Unity.Mathematics.Random(cur_time);

        rPos = new float2[amountAsteroids];
        nRPos = new NativeArray<float2>(amountAsteroids, Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        NativeArray<AsteroidData> asteroids = asteroidsQuery.ToComponentDataArray<AsteroidData>(Allocator.Temp);

        int amountToInstantiate = amountAsteroids - asteroids.Length;

        if (amountToInstantiate == 0) return;

        bool2 invert = random.NextBool2();
        float4 bounds = new float4(6f, 18f, 8f, 12.5f);

        for (int i = 0; i < amountToInstantiate; i++)
        {
            if (invert.x)
            {
                rPos[i].x = random.NextFloat(bounds.x, bounds.y) * (invert.y ? -1f : 1f);
                rPos[i].y = random.NextFloat(-bounds.w, bounds.w);
            }
            else
            {
                rPos[i].x = random.NextFloat(-bounds.y, bounds.y);
                rPos[i].y = random.NextFloat(bounds.z, bounds.w) * (invert.y ? -1f : 1f);
            }
        }

        nRPos.CopyFrom(rPos);
        NativeArray<float2> tempPos = nRPos;

        EntityCommandBuffer ecb = beginSimulation_ecbs.CreateCommandBuffer();        

        Entity prefab = asteroidPrefab;

        float maxSpeed = asteroidDataPrefab.maxSpeed;

        Job.WithCode(() =>
        {
            for (int i = 0; i < amountToInstantiate; i++)
            {
                Entity newAsteroid = ecb.Instantiate(prefab);                

                Translation translation = new Translation();
                translation.Value.x = tempPos[i].x;
                translation.Value.y = tempPos[i].y;

                ecb.SetComponent(newAsteroid, translation);
            }

        }).Schedule();

        asteroids.Dispose();

        beginSimulation_ecbs.AddJobHandleForProducer(Dependency);
    }

    protected override void OnDestroy()
    {

    }

    protected override void OnStopRunning()
    {
        nRPos.Dispose();
    }
}
