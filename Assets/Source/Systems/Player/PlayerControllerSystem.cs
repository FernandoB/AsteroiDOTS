using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerControllerSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate(GetEntityQuery(typeof(GameStateRunning)));
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        float hIn = -Input.GetAxis("Horizontal");
        float vIn = 0f;

        if(Input.GetKey(KeyCode.W))
        {
            vIn += Time.DeltaTime;
            if(vIn > 1f)
            {
                vIn = 1f;
            }
        }
        else {
            vIn = -1f;
        }

        Entities
            .WithNone<DisabledTag>()
            .ForEach((ref PlayerData player, ref Translation translation, ref Rotation rotation, in LocalToWorld local2World) =>
        {

            rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(math.radians(player.rotationSpeed * hIn * deltaTime)));

            if (vIn > 0f)
            {
                player.direction = player.direction + (local2World.Up * vIn * player.acceleration);

                if(math.length(player.direction) > player.maxSpeed)
                {
                    player.direction = math.normalize(player.direction) * player.maxSpeed;
                }
            }
            else
            {
                player.direction = player.direction * 0.99f;
            }

            translation.Value = translation.Value + (player.direction * deltaTime);

        }).Schedule();
    }
}
