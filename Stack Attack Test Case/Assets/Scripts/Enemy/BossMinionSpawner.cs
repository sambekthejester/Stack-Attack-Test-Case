using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossMinionSpawner : MonoBehaviour
{
    public enum WavePattern { Line, Fan, Rain, Surround }

     
    [SerializeField] GameObject minionPrefab;
     
    [SerializeField] Transform[] spawnPoints;

    
    [SerializeField] float withinWaveDelay = 0.2f;   

   
    [SerializeField] float ringRadius = 6f;          
    [SerializeField] int ringCount = 8;          
    [SerializeField] float fanAngle = 60f;     
    [SerializeField] float rainWidth = 12f;       
    [SerializeField] float rainZ = 18f;         

    int alive;

    public void ResetAliveCounter() => alive = 0;

    public int Alive => alive;

 
    public IEnumerator SpawnWave(WavePattern pattern, int count)
    {
        switch (pattern)
        {
            case WavePattern.Line: yield return SpawnLine(count); break;
            case WavePattern.Fan: yield return SpawnFan(count); break;
            case WavePattern.Rain: yield return SpawnRain(count); break;
            case WavePattern.Surround: yield return SpawnSurround(); break;
        }
    }
     
    IEnumerator SpawnLine(int count)
    {
        Transform src = ChooseSpawnPointOrSelf();
        for (int i = 0; i < count; i++)
        {
            SpawnOne(src.position, src.rotation);
            if (i < count - 1 && withinWaveDelay > 0f) yield return new WaitForSeconds(withinWaveDelay);
        }
    }
     
    IEnumerator SpawnFan(int count)
    {
        Transform src = ChooseSpawnPointOrSelf();
        if (count <= 1)
        {
            SpawnOne(src.position, src.rotation);
            yield break;
        }

        float start = -fanAngle * 0.5f;
        float step = fanAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float yaw = start + step * i;
            Quaternion rot = Quaternion.AngleAxis(yaw, Vector3.up) * src.rotation;
            SpawnOne(src.position, rot);
            if (i < count - 1 && withinWaveDelay > 0f) yield return new WaitForSeconds(withinWaveDelay);
        }
    }

    
    IEnumerator SpawnRain(int count)
    {
        Vector3 basePos = transform.position;
        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-rainWidth * 0.5f, rainWidth * 0.5f);
            Vector3 pos = new Vector3(x, 0 , rainZ);
            SpawnOne(pos, Quaternion.LookRotation(Vector3.back, Vector3.up)); 
            if (i < count - 1 && withinWaveDelay > 0f) yield return new WaitForSeconds(withinWaveDelay);
        }
    }

   
    IEnumerator SpawnSurround()
    {
        Vector3 center = transform.position;
        int c = Mathf.Max(3, ringCount);
        for (int i = 0; i < c; i++)
        {
            float t = (i / (float)c) * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(t), 0f, Mathf.Sin(t));
            Vector3 pos = center + dir * ringRadius;
            Quaternion rot = Quaternion.LookRotation(-dir, Vector3.up); 
            SpawnOne(pos, rot);
            if (i < c - 1 && withinWaveDelay > 0f) yield return new WaitForSeconds(withinWaveDelay);
        }
    }

 
    Transform ChooseSpawnPointOrSelf()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        return this.transform;
    }

    void SpawnOne(Vector3 pos, Quaternion rot)
    {
        if (!minionPrefab) return;
        var m = Instantiate(minionPrefab, pos, rot);
        alive++;

        var hp = m.GetComponent<EnemyHP>();
        if (hp != null) hp.OnDeath.AddListener(() => alive = Mathf.Max(0, alive - 1));
        else
        {
            var hook = m.AddComponent<OnDestroyHook>();
            hook.onDestroyed += () => alive = Mathf.Max(0, alive - 1);
        }
    }
}

public class OnDestroyHook : MonoBehaviour
{
    public System.Action onDestroyed;
    void OnDestroy() => onDestroyed?.Invoke();
}
