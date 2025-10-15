using UnityEngine;
using UnityEngine.Events;

public class BossHP : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 500;
    [SerializeField] int current;
    bool isBoss = false;

    [Header("Gates")]
    [SerializeField] bool invulnerableUntilBegin = true;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => current;
    public bool IsBoss => isBoss;

    public UnityEvent<int, int> OnHPChanged;  
    public UnityEvent OnDeath;

    public void AllowDamage() { invulnerableUntilBegin = false; } 

    void Awake()
    {
        if (maxHealth < 1) maxHealth = 1;
        if (current <= 0) current = maxHealth;
        OnHPChanged?.Invoke(current, maxHealth);
        isBoss = true;
    }
 

    public void TakeDamage(int amount)
    {
        // <<< BAÞLAMADAN ÖNCE HASARI YOK SAY
        if (invulnerableUntilBegin) return;

        if (current <= 0) return;
        current = Mathf.Max(0, current - Mathf.Max(0, amount));
        OnHPChanged?.Invoke(current, maxHealth);
        if (current <= 0) OnDeath?.Invoke();
    }
 
}
