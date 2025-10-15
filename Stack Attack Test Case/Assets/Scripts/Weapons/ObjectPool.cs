using UnityEngine;
using System.Collections.Generic;

 
public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform root;

 
    private readonly List<GameObject> free = new List<GameObject>(32);
     
    private readonly HashSet<GameObject> inUse = new HashSet<GameObject>();

    public int TotalCount => free.Count + inUse.Count;
    public int FreeCount => free.Count;
    public int InUseCount => inUse.Count;

    public ObjectPool(GameObject prefab, Transform root)
    {
        this.prefab = prefab;
        this.root = root;
    }

    public void Prewarm(int count)
    {
        EnsureFree(count);
    }

  
    public void SetTotalCapacity(int total)
    {
        int current = TotalCount;
        if (total > current)
        {
            EnsureFree(total - current);
        }
        else if (total < current)
        {
         
            int needToRemove = current - total;
            for (int i = 0; i < needToRemove && free.Count > 0; i++)
            {
                var go = PopFree();
                if (go) Object.Destroy(go);
            }
         
        }
    }

    public GameObject Get(Transform parent = null)
    {
        GameObject go = free.Count > 0 ? PopFree() : Object.Instantiate(prefab, root);
        go.transform.SetParent(parent, worldPositionStays: false);
        go.SetActive(true);
        inUse.Add(go);
        return go;
    }

    public void Release(GameObject go)
    {
        if (go == null) return;
        if (inUse.Remove(go))
        {
            go.SetActive(false);
            go.transform.SetParent(root, false);
            free.Add(go);
        }
        else
        {
            if (!free.Contains(go))
            {
                go.SetActive(false);
                go.transform.SetParent(root, false);
                free.Add(go);
            }
        }
    }

    public void ClearAll()
    {
        foreach (var go in free) if (go) Object.Destroy(go);
        free.Clear();
        foreach (var go in inUse) if (go) Object.Destroy(go);
        inUse.Clear();
    }

    
    private void EnsureFree(int addCount)
    {
        for (int i = 0; i < addCount; i++)
        {
            var go = Object.Instantiate(prefab, root);
            go.SetActive(false);
            free.Add(go);
        }
    }

    private GameObject PopFree()
    {
        int last = free.Count - 1;
        var go = free[last];
        free.RemoveAt(last);
        return go;
    }
}
