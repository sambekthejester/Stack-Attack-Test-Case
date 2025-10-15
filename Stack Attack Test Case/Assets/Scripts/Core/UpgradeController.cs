using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeController : MonoBehaviour
{
    [System.Serializable]
    public class OptionWidgets
    {
        public Button button;
        public TextMeshProUGUI label;
    }

    [Header("Options (3 adet)")]
    [SerializeField] private OptionWidgets[] options = new OptionWidgets[3];

    private struct Choice
    {
        public enum Kind { Upgrade, Unlock }
        public Kind kind;
        public WeaponBase target;
        public WeaponUpgrade upgrade;  
        public string label;
    }

    private Choice[] generated = new Choice[3];

    void OnEnable()
    {
        WireButtons();
        GenerateOptions();
    }

    void OnDisable()
    {
        UnwireButtons();
    }

    public void GenerateOptions()
    {
        var owned = FireController.Instance.GetOwnedWeapons();
        var locked = FireController.Instance.GetLockedWeapons();

        for (int i = 0; i < options.Length; i++)
        {
            bool doUnlock = locked.Count > 0 && Random.value < 0.5f;
            if (!doUnlock && owned.Count == 0 && locked.Count > 0) doUnlock = true;

            if (doUnlock)
            {
                var w = locked[Random.Range(0, locked.Count)];
                generated[i] = new Choice
                {
                    kind = Choice.Kind.Unlock,
                    target = w,
                    label = $"Yeni Silah Aç: <b>{w.name}</b>"
                };
            }
            else
            {
                
                if (owned.Count == 0 && locked.Count > 0)
                {
                    var w = locked[Random.Range(0, locked.Count)];
                    generated[i] = new Choice { kind = Choice.Kind.Unlock, target = w, label = $"Yeni Silah Aç: <b>{w.name}</b>" };
                }
                else
                {
                    var w = owned[Random.Range(0, owned.Count)];
                    var up = MakeRandomUpgrade(w);
                    generated[i] = new Choice
                    {
                        kind = Choice.Kind.Upgrade,
                        target = w,
                        upgrade = up,
                        label = $"Güçlendir: <b>{w.name}</b>\n{Describe(up)}"
                    };
                }
            }

            if (options[i]?.label) options[i].label.text = generated[i].label;
        }
    }

    void OnPick(int index)
    {
        if (index < 0 || index >= generated.Length) return;
        var c = generated[index];

        if (c.kind == Choice.Kind.Unlock)
        {
   
            bool ok = FireController.Instance.UnlockWeapon(c.target);
       
        }
        else  
        {
            FireController.Instance.ApplyUpgrade(c.target, c.upgrade);
            c.target.RestartFiring();

          
        }

        GameManager.Instance.ExitUpgradeStateAndContinue();
    }



    // ---- helpers ----
    WeaponUpgrade MakeRandomUpgrade(WeaponBase w)
    {
        bool isRocket = w is Rocket;
        int roll = isRocket ? Random.Range(0, 6) : Random.Range(0, 5);  

        var up = WeaponUpgrade.Default;
        switch (roll)
        {
            case 0: up.addDelay = Mathf.Max(0.1f, w.Delay * 0.25f); break; // +%25 FR (min +0.5)
            case 1: up.addDamage = Mathf.Max(1, Mathf.RoundToInt(w.Damage * 0.25f)); break; // +%25 DMG
            case 2: up.addProjectiles = 1; break;
            case 3: up.addPierce = 1; break;
            case 4: up.setCooldownSec = Mathf.Max(0.1f, Mathf.RoundToInt(w.Cooldown * 0.15f)); break;
            case 5: up.addSize = Mathf.Max(0.1f, Mathf.RoundToInt(w.Size * 0.2f)); break;
            }
        return up;
    }

    string Describe(WeaponUpgrade u)
    {
        var parts = new List<string>();

        if (u.addDelay != 0f) parts.Add($"+{u.addDelay:0.##} FireRate");
        if (u.addDamage != 0) parts.Add($"+{u.addDamage} DMG");

        if (u.addProjectiles != 0) parts.Add($"+{u.addProjectiles} Mermi");
        if (u.addPierce != 0) parts.Add($"+{u.addPierce} Delme");

        if (u.addSize != 0f) parts.Add($"+{u.addSize:0.##} Boyut"); 
        if (u.setCooldownSec >= 0) parts.Add($"CD - {u.setCooldownSec}s");

        return string.Join(" • ", parts);
    }


    void WireButtons()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i]?.button == null) continue;
            int ix = i;
            options[i].button.onClick.AddListener(() => OnPick(ix));
        }
    }

    void UnwireButtons()
    {
        for (int i = 0; i < options.Length; i++)
            if (options[i]?.button != null)
                options[i].button.onClick.RemoveAllListeners();
    }

}
