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

    private static void SetRandomArray_F2X(ref float2[] array, int amount, ref Unity.Mathematics.Random random, float min, float max)
    {
        for (int i = 0; i < amount; i++)
        {
            array[i].x = random.NextFloat(min, max);
        }
    }

    private static void SetRandomArray_F2Y(ref float2[] array, int amount, ref Unity.Mathematics.Random random, float min, float max)
    {
        for (int i = 0; i < amount; i++)
        {
            array[i].y = random.NextFloat(min, max);
        }
    }

    protected override void OnUpdate()
    {
        NativeArray<AsteroidData> asteroids = asteroidsQuery.ToComponentDataArray<AsteroidData>(Allocator.Temp);

        int amountToInstantiate = amountAsteroids - asteroids.Length;

        if (amountToInstantiate == 0) return;

        SetRandomArray_F2X(ref rPos, amountToInstantiate, ref random, 6f, 18f);
        SetRandomArray_F2Y(ref rPos, amountToInstantiate, ref random, -12.5f, 12.5f);

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
