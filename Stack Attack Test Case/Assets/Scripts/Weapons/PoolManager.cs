using UnityEngine;
using System.Collections.Generic;

 
public class PoolManager : MonoSingleton<PoolManager>
{
     

 
    private readonly Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();
 
    private readonly Dictionary<GameObject, ObjectPool> instanceToPool = new Dictionary<GameObject, ObjectPool>();

     
    public void Preload(GameObject prefab, int count)
    {
        GetOrCreatePool(prefab).Prewarm(count);
    }

    public void EnsureCapacity(GameObject prefab, int total)
    {
        GetOrCreatePool(prefab).SetTotalCapacity(total);
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, Vector3? uniformScale = null, Transform parent = null)
    {
        var pool = GetOrCreatePool(prefab);
        var instance = pool.Get(parent);
        instanceToPool[instance] = pool;

        instance.transform.SetPositionAndRotation(pos, rot);
        if (uniformScale.HasValue) instance.transform.localScale = uniformScale.Value;

        instance.SetActive(true);         
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;
        if (instanceToPool.TryGetValue(instance, out var pool))
        {
            pool.Release(instance);          
        }
        else
        {
            instance.SetActive(false);
        }
    }


    private ObjectPool GetOrCreatePool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out var pool))
        {
            var root = new GameObject($"{prefab.name}_PoolRoot").transform;
            DontDestroyOnLoad(root.gameObject);
            pool = new ObjectPool(prefab, root);
            pools[prefab] = pool;
        }
        return pool;
    }
}
