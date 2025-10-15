using UnityEngine;

 
public class EnemyController : MonoBehaviour
{
    
    [SerializeField] float speedZ = 3f;       

    
    [SerializeField] int touchDamage = 1;     

    EnemyHP hp;
    EnemyText hpText;
    bool isDead;

    [SerializeField] GameObject DestroyedEnemy;


    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

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

   

    void Die()
    {
        if (isDead) return;
        isDead = true;

        GameObject tempCube = Instantiate(DestroyedEnemy, transform.position, Quaternion.identity);

        tempCube.transform.GetChild(0).transform.GetComponent<Rigidbody>().AddForce(new Vector3(10f, 10f, 10f) * Random.RandomRange(-1f, 0f), ForceMode.Force);
        tempCube.transform.GetChild(1).transform.GetComponent<Rigidbody>().AddForce(new Vector3(10f, 10f, 10f) * Random.RandomRange(0f, 1f), ForceMode.Force);
   
        Destroy(this.gameObject);
        Destroy(tempCube.transform.GetChild(0).gameObject, 0.8f);
        Destroy(tempCube.transform.GetChild(1).gameObject, 0.8f);
        Destroy(tempCube, 1f);

         
    }
}