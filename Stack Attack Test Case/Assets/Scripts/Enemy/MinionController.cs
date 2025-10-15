using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(EnemyHP))]
public class MinionController : MonoBehaviour
{
    [Header("Movement (Z only)")]
    [SerializeField] float speedZ = 3f;

    [Header("Combat")]
    [SerializeField] int touchDamage = 1;

    EnemyHP hp;
    EnemyText hpText;
    bool isDead;

    void Awake()
    { 
        var col = GetComponent<Collider>();
        col.isTrigger = true;

         
        hp = GetComponent<EnemyHP>();
        hpText = GetComponent<EnemyText>();
        if (hpText == null) hpText = gameObject.AddComponent<EnemyText>();

      
        hp.OnHPChanged.AddListener(hpText.SetHP);
        hp.OnDeath.AddListener(Die);
    }
    void Start()
    {
 
        hpText.SetHP(hp.CurrentHealth, hp.MaxHealth);
    }

    void Update()
    {
   
        transform.position += Vector3.back * speedZ * Time.deltaTime;
    }


    void OnTriggerEnter(Collider other)
    {
         
        var player = other.GetComponent<PlayerHurt>();
        if (player != null)
        {
            player.TryHit(touchDamage, player.DefaultInvincibilityAfterHit);
            Die();
            return;
        }

        
        if (other.CompareTag("EnemyDespawnBoundary"))
        {
            Die();
            return;
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

       
        Destroy(gameObject);
    }
}
