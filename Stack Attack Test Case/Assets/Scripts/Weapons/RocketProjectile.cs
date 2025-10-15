using UnityEngine;

public class RocketProjectile : Projectile
{
    [Header("Explosion")]
    [SerializeField] float explosionRadius = 2f;
    [SerializeField] private LayerMask damageMask;        
    [SerializeField] private GameObject explosionVFX;      
    [SerializeField] private float vfxLifetime = 0.6f;

 
    public void SetExplosionRadius(float r)
    {
        explosionRadius = Mathf.Max(0.1f, r);
    }


    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DespawnBoundary"))
        {
            ReturnToPool();
            return;
        }

        var enemyHP = other.GetComponent<EnemyHP>();
        if (enemyHP != null)
        {
            Explode();
            return;
        }
    }

    private void Explode()
    {
      

        Vector3 pos = transform.position;

 
        Collider[] hits = Physics.OverlapSphere(pos, explosionRadius, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            var hp = hits[i].GetComponent<IDamageable>();
            if (hp != null) hp.TakeDamage(damage);

            if (hp.CurrentHealth <= 0 && !hp.IsBoss)
                GameManager.Instance?.RegisterKill();
        }

    
        if (explosionVFX != null)
        {
            var fx = Instantiate(explosionVFX, pos, Quaternion.identity);
            fx.transform.localScale = Vector3.one * (explosionRadius * 2f); 
            Destroy(fx, vfxLifetime);
        }

        ReturnToPool();
    }
}
