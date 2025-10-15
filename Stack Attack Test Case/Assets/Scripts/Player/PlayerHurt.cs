using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHurt : MonoBehaviour
{
 
    [SerializeField] private int maxHealth = 3;   
    [SerializeField] private int current;          

    
    [SerializeField] private float defaultInvincibilityAfterHit = 1.5f;
    [SerializeField] private bool isInvincible = false;
    private Coroutine invRoutine;
    [SerializeField] private GameObject barier;


 public UnityEvent<float> OnHealthPercent;

   
    public UnityEvent<int, int> OnHeartsChanged;

    public UnityEvent OnDeath;
    public UnityEvent OnInvincibleStart;
    public UnityEvent OnInvincibleEnd;

    void Awake() => ResetFull();

    public void ResetFull()
    {
        current = Mathf.Max(1, maxHealth);
        OnHealthPercent?.Invoke(1f);
        OnHeartsChanged?.Invoke(current, maxHealth);
        SetInvincible(false);
    }

 
    public bool TryHit(int damage, float invDuration)
    {
        if (isInvincible || current <= 0) return false;

        current = Mathf.Max(0, current - Mathf.Max(0, damage));
        OnHealthPercent?.Invoke((float)current / maxHealth);
        OnHeartsChanged?.Invoke(current, maxHealth);

        if (current <= 0)
        {
            if (invRoutine != null) { StopCoroutine(invRoutine); invRoutine = null; }
            SetInvincible(false);
            OnDeath?.Invoke();
            return true;
        }

        if (invRoutine != null) StopCoroutine(invRoutine);
        if (invDuration > 0f)
            invRoutine = StartCoroutine(InvincibilityFor(invDuration));
        return true;
    }

    public void Heal(int amount)
    {
        if (current <= 0) return;
        current = Mathf.Min(maxHealth, current + Mathf.Max(0, amount));
        OnHealthPercent?.Invoke((float)current / maxHealth);
        OnHeartsChanged?.Invoke(current, maxHealth);
    }

    public void SetMaxHealth(int hearts, bool fill = true)
    {
        maxHealth = Mathf.Max(1, hearts);
        if (fill) current = maxHealth;
        OnHealthPercent?.Invoke((float)current / maxHealth);
        OnHeartsChanged?.Invoke(current, maxHealth);
    }

    IEnumerator InvincibilityFor(float seconds)
    {
        SetInvincible(true);
        yield return new WaitForSeconds(seconds);
        SetInvincible(false);
        invRoutine = null;
    }

    void SetInvincible(bool v)
    {
        if (isInvincible == v) return;
        isInvincible = v;
        barier.SetActive(isInvincible);
        if (v) OnInvincibleStart?.Invoke();
        else OnInvincibleEnd?.Invoke();
    }

 
    public float DefaultInvincibilityAfterHit => defaultInvincibilityAfterHit;
    public int Current => current;
    public int Max => maxHealth;
    public bool IsInvincible => isInvincible;
}
