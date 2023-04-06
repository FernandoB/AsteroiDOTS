using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveAsteroidsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation translation, in Rotation rotation, in AsteroidData asteroid) => {
                 translation.Value.x += 0.5f * deltaTime;
        }).Schedule();
    }
}
