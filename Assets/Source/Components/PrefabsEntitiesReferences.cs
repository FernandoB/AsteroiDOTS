using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PrefabsEntitiesReferences : IComponentData
{
    public Entity asteroidBigEntityPrefab;

    public Entity asteroidMediumEntityPrefab;

    public Entity asteroidSmallEntityPrefab;

    public Entity playerEntityPrefab;

    public Entity bulletEntityPrefab;

    public Entity alienShipBigEntityPrefab;

    public Entity alienShipSmallEntityPrefab;

    public Entity shieldEntityPrefab;

    public Entity powerUpShieldEntityPrefab;

    public Entity powerupWeaponAEntityPrefab;
}
