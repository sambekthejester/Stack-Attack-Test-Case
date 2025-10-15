using UnityEngine;
using System.Collections.Generic;
 
[System.Serializable]
public class WeaponEntry
{
    public WeaponBase weapon;   
    public bool locked = false;

    [HideInInspector] public bool defaultLocked;  
}

public class FireController : MonoSingleton<FireController>
{
   
    [Header("Weapons")]
    
    [SerializeField] private List<WeaponEntry> weapons = new List<WeaponEntry>();

    
    [SerializeField] private bool autoStartFiring = false; 

    bool isFiring;

    public bool IsFiring => isFiring;
  

    protected override void Awake()
    {
        base.Awake();
        foreach (var e in weapons)
            if (e != null) e.defaultLocked = e.locked;

    }


     


    
    public void StartAll()
    {
        if (isFiring) return;                 
        isFiring = true;

        foreach (var e in weapons)
        {
            if (e.weapon == null || e.locked) continue;
            e.weapon.HoldFire = false;
            e.weapon.StartFiring();
        }
     
    }

 
    public void StopAll()
    {
        if (!isFiring) return;
        isFiring = false;

        foreach (var e in weapons)
            if (e.weapon != null) e.weapon.StopFiring();
     
    }
  
    public void SetHoldFireAll(bool v)
    {
        foreach (var e in weapons)
            if (e.weapon != null && !e.locked)
                e.weapon.HoldFire = v;

         
    }

    public void RestartAllFiring()
    {
        foreach (var e in weapons)
            if (e.weapon != null && !e.locked)
                e.weapon.RestartFiring();
 
    }


    public void ResetAllWeaponsForNewRun()
    {
     
        StopAll();

        foreach (var wep in weapons)
        {
            if (wep.weapon == null) continue;

          
            wep.weapon.ResetToBaseline();

           
            wep.locked = wep.defaultLocked;

       
            if (wep.locked) wep.weapon.StopFiring();
        }

    
        isFiring = false;
    }


     
    public void UnlockByIndex(int index)
    {
        if (index < 0 || index >= weapons.Count) return;
        var wep = weapons[index];
        if (wep.weapon == null) return;

        wep.locked = false;
        wep.weapon.HoldFire = false;
        if (isFiring) wep.weapon.StartFiring();
    }

  
    public void LockWeapon(WeaponBase wep, bool locked)
    {
        var entry = weapons.Find(x => x.weapon == wep);
        if (entry == null || entry.weapon == null) return;

        entry.locked = locked;
        if (locked)
        {
            entry.weapon.StopFiring();
        }
        else
        {
            entry.weapon.HoldFire = false;
            if (isFiring) entry.weapon.StartFiring();
        }
    }

   
    public void AddWeapon(WeaponBase wep, bool locked = true)
    {
        if (wep == null) return;
        weapons.Add(new WeaponEntry { weapon = wep, locked = locked });
        if (!locked)
        {
            wep.HoldFire = false;
            if (isFiring) wep.StartFiring();
        }
    }

     
    public void RemoveWeapon(WeaponBase wep)
    {
        var entry = weapons.Find(x => x.weapon == wep);
        if (entry == null) return;
        if (entry.weapon != null) entry.weapon.StopFiring();
        weapons.Remove(entry);
    }

  
    public List<WeaponBase> GetOwnedWeapons()
    {
        var list = new List<WeaponBase>();
        foreach (var a in weapons)
            if (a.weapon != null && !a.locked) list.Add(a.weapon);
        return list;
    }
     
    public List<WeaponBase> GetLockedWeapons()
    {
        var list = new List<WeaponBase>();
        foreach (var a in weapons)
            if (a.weapon != null && a.locked) list.Add(a.weapon);
        return list;
    }

    
    public bool UnlockWeapon(WeaponBase targetInstance)
    {
         
        if (targetInstance == null)
        {
            Debug.LogWarning("UnlockWeapon: target null");
            return false;
        }

       
        foreach (var wep in weapons)
        {
            if (wep.weapon == targetInstance)
            {
                if (!wep.locked)
                {
                    Debug.Log($"UnlockWeapon: {targetInstance.name} zaten açýk.");
                    return false;
                }

        
                wep.locked = false;

                
                if (isFiring)
                {
                    targetInstance.HoldFire = false;
                    targetInstance.RestartFiring(); 
                    
                }

           
            
                return true;
            }
        }

        Debug.LogWarning($"UnlockWeapon: hedef listede yok! (Muhtemelen prefab referansý verildi)");
        return false;
    }
    
    public void ApplyUpgrade(WeaponBase a, WeaponUpgrade up)
    {
        if (a == null) return;
        a.ApplyUpgrade(up);
        if (!a.HoldFire) a.RestartFiring();

    }

 

    public WeaponBase GetWeaponByIndex(int i)
    {
        if (i < 0 || i >= weapons.Count) return null;
        return weapons[i].weapon;
    }

    public WeaponBase GetWeaponByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var e = weapons.Find(x => x.weapon != null && x.weapon.name == name);
        return e != null ? e.weapon : null;
    }

    public IReadOnlyList<WeaponEntry> Weapons => weapons;
     
}
