using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

public class CollidePlayerBulletSystem : SystemCollideGeneric<PlayerData, HitTag, BulletData, DeleteTag>
{

}
