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

    NativeArray<float2> nRPos;
    NativeArray<float3> rDir;
    NativeArray<float> rSpeed;

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

        nRPos = new NativeArray<float2>(amountAsteroids, Allocator.Persistent);
        rDir = new NativeArray<float3>(amountAsteroids, Allocator.Persistent);
        rSpeed = new NativeArray<float>(amountAsteroids, Allocator.Persistent);
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
            float2 tempN;
            if (invert.x)
            {
                tempN.x = random.NextFloat(bounds.x, bounds.y) * (invert.y ? -1f : 1f);
                tempN.y = random.NextFloat(-bounds.w, bounds.w);
            }
            else
            {
                tempN.x = random.NextFloat(-bounds.y, bounds.y);
                tempN.y = random.NextFloat(bounds.z, bounds.w) * (invert.y ? -1f : 1f);
            }
            nRPos[i] = tempN;
        }

        for (int i = 0; i < amountToInstantiate; i++)
        {
            float3 dir = random.NextFloat3Direction();
            dir.z = 0f;
            rDir[i] = dir;

            rSpeed[i] = random.NextFloat(2f, 6f);
        }

        NativeArray<float2> tempPos = nRPos;
        NativeArray<float3> tempDir = rDir;
        NativeArray<float> tempSpeed = rSpeed;

        EntityCommandBuffer ecb = beginSimulation_ecbs.CreateCommandBuffer();        

        Entity prefab = asteroidPrefab;

        Job.WithCode(() =>
        {
            for (int i = 0; i < amountToInstantiate; i++)
            {
                Entity newAsteroid = ecb.Instantiate(prefab);                

                Translation translation = new Translation();
                translation.Value.x = tempPos[i].x;
                translation.Value.y = tempPos[i].y;

                AsteroidData asteroidData = new AsteroidData();
                asteroidData.direction = tempDir[i];
                asteroidData.speed = tempSpeed[i];

                ecb.SetComponent(newAsteroid, translation);
                ecb.SetComponent(newAsteroid, asteroidData);
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
        rDir.Dispose();
        rSpeed.Dispose();
    }
}
