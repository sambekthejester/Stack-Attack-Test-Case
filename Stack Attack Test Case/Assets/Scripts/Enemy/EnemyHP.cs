using UnityEngine;
using UnityEngine.Events;

public class EnemyHP : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] int current;
    bool isBoss = false;

 
    [SerializeField] bool randomizeFromGameManager = true;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => current;
    public bool IsBoss => isBoss;

    public UnityEvent<int, int> OnHPChanged; 
    public UnityEvent OnDeath;

   


    void OnEnable()
    {
        isBoss = false; 

        if (randomizeFromGameManager && GameManager.Instance != null)
        {
            GameManager.Instance.GetEnemyHPRandomRange(out int min, out int max);
            maxHealth = Random.Range(min, max + 1);
            current = maxHealth;
        }
        else
        {
            if (maxHealth < 1) maxHealth = 1;
            if (current <= 0) current = maxHealth;
        }

        OnHPChanged?.Invoke(current, maxHealth);
    }
 
    public void TakeDamage(int amount)
    {
        if (current <= 0) return;
        current = Mathf.Max(0, current - Mathf.Max(0, amount));
        OnHPChanged?.Invoke(current, maxHealth);
        if (current <= 0) OnDeath?.Invoke();
    }

  
    
}
public interface IDamageable
{
    int MaxHealth { get; }
    int CurrentHealth { get; }
    void TakeDamage(int amount);
    bool IsBoss { get; }

}