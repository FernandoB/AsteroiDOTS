using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BulletMoveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation translation, in LocalToWorld localToWorld, in BulletData bulletData) =>
        {
            translation.Value = translation.Value + ((bulletData.startVelocity + (bulletData.direction * bulletData.maxSpeed)) * deltaTime);

        }).ScheduleParallel();
    }
}
