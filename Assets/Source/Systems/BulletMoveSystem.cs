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
            Debug.Log(string.Format("{0}, {1}, {2}, {3}", bulletData.startVelocity, localToWorld.Up, bulletData.maxSpeed, deltaTime));
            translation.Value = translation.Value + ((bulletData.startVelocity + (localToWorld.Up * bulletData.maxSpeed)) * deltaTime);

        }).Schedule();
    }
}
