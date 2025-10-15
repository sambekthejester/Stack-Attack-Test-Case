using UnityEngine;
using System.Collections;
public class BasicGun : WeaponBase
{
    [Header("Shotgun-Style X Spread")]
    [SerializeField] private float lateralSpacing = 0.5f;
    [SerializeField] private bool useTotalWidth = false;
    [SerializeField] private float totalWidth = 1.5f;



    protected override IEnumerator FireBurst()
    {
        int n = Mathf.Max(1, ProjectileCount);
        Vector3 fwd = PlanarForward();
        Vector3 right = Vector3.ProjectOnPlane(firePoint.right, Vector3.up).normalized;
        if (right.sqrMagnitude < 1e-6f) right = Vector3.right;

        float[] offsets = BuildOffsets(n);

        for (int i = 0; i < n; i++)
        {
            Vector3 spawnPos = firePoint.position + right * offsets[i];
            SpawnProjectileInstanceAt(spawnPos, fwd, projectileSpeed);
        }

        yield break;  
    }


    protected override void SpawnOneShot()
    {
       
        SpawnProjectileInstance(PlanarForward(), projectileSpeed);
    }

    float[] BuildOffsets(int n)
    {
        var arr = new float[n];
        if (n == 1) { arr[0] = 0f; return arr; }

        if (useTotalWidth)
        {
            float start = -0.5f * totalWidth;
            float step = totalWidth / (n - 1);
            for (int i = 0; i < n; i++) arr[i] = start + i * step;
        }
        else
        {
            float start = -0.5f * (n - 1) * lateralSpacing;
            for (int i = 0; i < n; i++) arr[i] = start + i * lateralSpacing;
        }
        return arr;
    }
}
