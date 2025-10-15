using UnityEngine;

 
public class Projectile : MonoBehaviour
{
    [SerializeField] float lifeTime = 6f;

    protected Rigidbody rb;
    protected int damage;
    protected int pierceLeft;
    protected float fixedY;
    bool _returned;

    void OnEnable()
    {
        _returned = false;          
        
        CancelInvoke(nameof(ReturnToPool));
    }
    void OnDisable()
    {
    
        CancelInvoke(nameof(ReturnToPool));
        _returned = false;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezePositionY;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public virtual void Init(int dmg, int pierce, Vector3 planarVelocity, float fixedY)
    {
        damage = dmg;
        pierceLeft = pierce;
        this.fixedY = fixedY;

        planarVelocity.y = 0f;
        rb.linearVelocity = planarVelocity;

        var p = transform.position; p.y = fixedY; transform.position = p;

        CancelInvoke(nameof(ReturnToPool));
        if (lifeTime > 0f) Invoke(nameof(ReturnToPool), lifeTime);
        else ReturnToPool();
    }

    protected virtual void FixedUpdate()
    {
        var v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v;
        var p = rb.position; p.y = fixedY; rb.position = p;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("DespawnBoundary"))
        {
            ReturnToPool();
            return;
        }
        var enemyHP = other.GetComponent<IDamageable>();
        if (enemyHP != null)
        {
            enemyHP.TakeDamage(damage);

            if (enemyHP.CurrentHealth <= 0 && !enemyHP.IsBoss)
                GameManager.Instance?.RegisterKill();

            if (pierceLeft > 0) { pierceLeft--; return; }
            ReturnToPool();
            return;
        }
    }

    protected void ReturnToPool()
    {
        if (_returned) return;      
        _returned = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        PoolManager.Instance.Despawn(gameObject);
      
    }
    public void ForceDespawn()
    {
       
        CancelInvoke(nameof(ReturnToPool));
        ReturnToPool();  
    }

  
    public static void DespawnAllActive()
    {
        var all = FindObjectsOfType<Projectile>(includeInactive: false);
        for (int i = 0; i < all.Length; i++)
            all[i].ForceDespawn();
    }

}

 
