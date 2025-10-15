using System.Collections;
using UnityEngine;

public class Rocket : WeaponBase
{

    protected override void SpawnOneShot()
    {
        float yaw = Random.Range(-spreadAngle * 0.3f, spreadAngle * 0.3f);
        Vector3 dir = SpreadDir(yaw);
        SpawnProjectileInstance(dir, projectileSpeed);
    }
     
}
 
 