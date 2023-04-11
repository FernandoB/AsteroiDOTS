using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class WorldBoundsSystem : SystemBase
{
    private float offset = 0.5f;

    private Camera cam;
    private Vector3 camP;

    private float aspect;

    private float worldHeight;
    private float worldWidth;

    protected override void OnStartRunning()
    {
        cam = Camera.main;
        camP = cam.transform.position;
    }

    protected override void OnUpdate()
    {
        aspect = (float)Screen.width / Screen.height;

        worldHeight = cam.orthographicSize * 2;

        worldWidth = worldHeight * aspect;

        float2 minMaxX = new float2(camP.x - (worldWidth / 2f) - offset, camP.x + (worldWidth / 2f) + offset);
        float2 minMaxY = new float2(camP.y - (worldHeight / 2f) - offset, camP.y + (worldHeight / 2f) + offset);

        Entities
            .WithNone<DisabledTag>()
            .ForEach((ref Translation translation) =>
        {
            float3 pos = translation.Value;

            if(pos.x > minMaxX.y)
            {
                pos.x = minMaxX.x;
            }
            if (pos.x < minMaxX.x)
            {
                pos.x = minMaxX.y;
            }
            if(pos.y > minMaxY.y)
            {
                pos.y = minMaxY.x;
            }
            if (pos.y < minMaxY.x)
            {
                pos.y = minMaxY.y;
            }

            translation.Value = pos;

        }).ScheduleParallel();
    }
}
