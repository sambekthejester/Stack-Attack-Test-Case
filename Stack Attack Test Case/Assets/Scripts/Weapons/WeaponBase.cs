using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
   
    [SerializeField] private float cooldown = 0f;      
    [SerializeField] private int projectileCount = 1;   
    [SerializeField] private int damage = 10;
    [SerializeField] private int pierceCount = 0;
    [SerializeField] private float delay = 0f;       
    [SerializeField] private float size = 1f;          

  
    [SerializeField] protected bool holdFire = false;

  
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float spreadAngle = 3f;
    [SerializeField] protected float projectileSpeed = 40f;

    protected Coroutine fireLoop;
    protected bool isCoolingDown;

    public static System.Action OnAnyWeaponFired;

    [System.Serializable]
    struct Baseline
    {
        public float cooldown, delay, size;
        public int projectileCount, damage, pierceCount;
        public bool holdFire;
    }
    Baseline _baseline;

   
    public float Cooldown { get => cooldown; set => cooldown = Mathf.Max(0f, value); }
    public int ProjectileCount { get => projectileCount; set => projectileCount = Mathf.Max(1, value); }
    public int Damage { get => damage; set => damage = Mathf.Max(0, value); }
    public int PierceCount { get => pierceCount; set => pierceCount = Mathf.Max(0, value); }
    public float Delay { get => delay; set => delay = Mathf.Max(0f, value); }
    public float Size { get => size; set => size = Mathf.Max(0.01f, value); }  
    public bool HoldFire { get => holdFire; set => holdFire = value; }
    public bool IsFiring => fireLoop != null;

    protected virtual void Awake()
    {
        if (firePoint == null) firePoint = transform;
 
        _baseline = new Baseline
        {
            cooldown = cooldown,
            delay = delay,
            size = size,
            projectileCount = projectileCount,
            damage = damage,
            pierceCount = pierceCount,
            holdFire = holdFire
        };
        OnCaptureBaseline(); 
    }


    public virtual void StartFiring()
    {
        if (fireLoop != null) return;
        fireLoop = StartCoroutine(FireLoop());
    }

    public virtual void StopFiring()
    {
        if (fireLoop != null)
        {
            StopCoroutine(fireLoop);
            fireLoop = null;
        }
        isCoolingDown = false;
    }


    public void RestartFiring()
    {
        if (fireLoop != null)
        {
            StopCoroutine(fireLoop);
            fireLoop = null;
        }
        isCoolingDown = false;
        if (!HoldFire)
            fireLoop = StartCoroutine(FireLoop());
    }

    IEnumerator FireLoop()
    {
        while (true)
        {
            if (!HoldFire && !isCoolingDown)
            {
                yield return StartCoroutine(FireBurst());

                if (Cooldown > 0f)
                {
                    isCoolingDown = true;
                    yield return new WaitForSeconds(Cooldown);
                    isCoolingDown = false;
                }
            }

            yield return null;
        }
    }

    protected virtual IEnumerator FireBurst()
    {
        int n = Mathf.Max(1, ProjectileCount);
        

        for (int i = 0; i < n; i++)
        {
            SpawnOneShot();
            if (i < n - 1 && delay > 0f)
                yield return new WaitForSeconds(delay);
        }
    }

    protected abstract void SpawnOneShot();

    protected Vector3 PlanarForward()
    {
        Vector3 f = Vector3.ProjectOnPlane(firePoint.forward, Vector3.up);
        return f.sqrMagnitude > 1e-6f ? f.normalized : Vector3.forward;
    }

    protected Vector3 SpreadDir(float degree)
    {
        return Quaternion.AngleAxis(degree, Vector3.up) * PlanarForward();
    }

    protected void SpawnProjectileInstance(Vector3 dir, float speed)
    {
        SpawnProjectileInstanceAt(firePoint.position, dir, speed);
    }

    protected void SpawnProjectileInstanceAt(Vector3 position, Vector3 dir, float speed)
    {
        float y = firePoint.position.y;  
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        GameObject go = PoolManager.Instance.Spawn(
            projectilePrefab,
            position,
            rot,
            projectilePrefab.transform.localScale  
        );

        var proj = go.GetComponent<Projectile>();
        if (proj == null)
        {
            PoolManager.Instance.Despawn(go);
            return;
        }

        proj.Init(Damage, PierceCount, dir * speed, y);

        OnAnyWeaponFired?.Invoke();

        var rocket = proj as RocketProjectile;
        if (rocket != null)
            rocket.SetExplosionRadius(Size);

    }

  
    public void ApplyUpgrade(WeaponUpgrade u)
    {
        
        if (u.setCooldownSec >= 0f) Cooldown = Mathf.Max(0, Mathf.RoundToInt(Cooldown - u.setCooldownSec));

        if (u.addDamage != 0f) Damage = Mathf.Max(0, Mathf.RoundToInt(Damage + u.addDamage));
        
        if (u.addDelay != 0f) Delay = Mathf.Max(0f, Delay + u.addDelay);
        

        if (u.addProjectiles != 0f) ProjectileCount = Mathf.Max(1, Mathf.RoundToInt(ProjectileCount + u.addProjectiles));
        if (u.addPierce != 0f) PierceCount = Mathf.Max(0, Mathf.RoundToInt(PierceCount + u.addPierce));

         
        if (u.addSize != 0f) Size = Mathf.Max(0.01f, Size + u.addSize);
        

       
    }
    public virtual void ResetToBaseline()
    {
        StopFiring();

        Cooldown = _baseline.cooldown;
        Damage = _baseline.damage;

        Delay = _baseline.delay;
      
        ProjectileCount = _baseline.projectileCount;
        PierceCount = _baseline.pierceCount;
        Size = _baseline.size;

        
        HoldFire = _baseline.holdFire;

        OnRestoreBaseline(); 
    }

    protected virtual void OnCaptureBaseline() { }
    protected virtual void OnRestoreBaseline() { }
}

 public struct WeaponUpgrade
{
    public int addDamage;
    
    public float addDelay;
    
    public int addProjectiles;
    public int addPierce;
    public float addSize;      
     
    public float setCooldownSec; // -1 => ignore

    public static WeaponUpgrade Default => new WeaponUpgrade { setCooldownSec = -1f };
}