﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AsteroidsMoveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .WithAll<AsteroidData>()
            .ForEach((ref Translation translation, in AsteroidData asteroidData) =>
        {
            translation.Value = translation.Value + (asteroidData.direction * asteroidData.speed * deltaTime);

        }).ScheduleParallel();
    }
}