using UnityEngine;
using System.Collections;

public class Uzi : WeaponBase
{

    protected override void SpawnOneShot()
    {
        float yaw = Random.Range(-spreadAngle, spreadAngle);
        Vector3 dir = SpreadDir(yaw);
        SpawnProjectileInstance(dir, projectileSpeed);
    }
    
}
