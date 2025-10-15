using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class BossController : MonoBehaviour
{
    
    [SerializeField] float startDelayAfterBoundary = 3f; 

    
    [SerializeField] float preStartSpeedZ = 3f;           

    [Header("Waves")]
    [SerializeField] BossMinionSpawner spawner;
    [SerializeField] float waveInterval = 1.8f;
    [SerializeField] int lineCountPerWave = 6;
    [SerializeField] int fanCountPerWave = 7;
    [SerializeField] int rainCountPerWave = 9;
    [SerializeField]
    BossMinionSpawner.WavePattern[] patternLoop = {BossMinionSpawner.WavePattern.Line, BossMinionSpawner.WavePattern.Fan,
                                                                      BossMinionSpawner.WavePattern.Rain, BossMinionSpawner.WavePattern.Surround };

   
    [SerializeField] bool isProgressBar = true;
    Transform despawnBoundary;

    [SerializeField] ParticleSystem deathVFX;

  
    bool started, queued, hitBoundary, frozen;
    float startZ;      
    float boundaryZ;    
    Coroutine waveCo;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!spawner) spawner = GetComponent<BossMinionSpawner>();

        if (!despawnBoundary)
        {
            var go = GameObject.FindGameObjectWithTag("BossBoundary");
            if (go) despawnBoundary = go.transform;
        }

         
        startZ = transform.position.z;
        boundaryZ = despawnBoundary ? despawnBoundary.position.z : startZ;  

        started = false; queued = false; hitBoundary = false; frozen = false;

        if (isProgressBar) UIManager.Instance?.SetProgress(0f);


        var hp = GetComponent<BossHP>();
        if (hp != null)
            hp.OnDeath.AddListener(OnBossDeath);
    }
   

    void Update()
    {
         
        if (!hitBoundary && !frozen)
        {
            transform.position += Vector3.back * preStartSpeedZ * Time.deltaTime;
        }
        
        if (isProgressBar)
        {
            if (hitBoundary)
            {
                UIManager.Instance?.SetProgress(1f);
            }
            else
            {
               
                float p = Mathf.InverseLerp(startZ, boundaryZ, transform.position.z);
                UIManager.Instance?.SetProgress(Mathf.Clamp01(p));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("BossBoundary")) return;

        
        if (!hitBoundary)
        {
            hitBoundary = true;
            boundaryZ = transform.position.z;  
            if (isProgressBar) UIManager.Instance?.SetProgress(1f);
        }

       
        frozen = true;

      
        if (!started && !queued)
        {
            queued = true;
            StartCoroutine(BeginAfterDelay());
        }

    }

    IEnumerator BeginAfterDelay()
    {
        yield return new WaitForSeconds(startDelayAfterBoundary);
        BeginBoss();
    }
    void BeginBoss()
    {
        if (started) return;
        started = true;

        var hp = GetComponent<BossHP>();
        if (hp) hp.AllowDamage();
        
        if (spawner) spawner.ResetAliveCounter();
        if (waveCo != null) StopCoroutine(waveCo);
        waveCo = StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        int ix = 0;
        var wfs = new WaitForSeconds(waveInterval);

        while (true)
        {
            var pat = (patternLoop != null && patternLoop.Length > 0)
                ? patternLoop[ix++ % patternLoop.Length]
                : (BossMinionSpawner.WavePattern)Random.Range(0, 4);

            if (spawner != null)
            {
                switch (pat)
                {
                    case BossMinionSpawner.WavePattern.Line: yield return spawner.SpawnWave(pat, lineCountPerWave); break;
                    case BossMinionSpawner.WavePattern.Fan: yield return spawner.SpawnWave(pat, fanCountPerWave); break;
                    case BossMinionSpawner.WavePattern.Rain: yield return spawner.SpawnWave(pat, rainCountPerWave); break;
                    case BossMinionSpawner.WavePattern.Surround: yield return spawner.SpawnWave(pat, 0); break;
                }
            }

            yield return wfs;
        }
    }

    public void OnBossDeath()
    {
        started = false;
        if (waveCo != null) { StopCoroutine(waveCo); waveCo = null; }

        if (isProgressBar) UIManager.Instance?.SetProgress(1f);

        StartCoroutine(DeathSequence());
    }
    IEnumerator DeathSequence()
    {
     
        started = false;
        if (waveCo != null) { StopCoroutine(waveCo); waveCo = null; }

       
        var hp = GetComponent<BossHP>();
        if (hp) hp.enabled = false;

        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        if (spawner) spawner.enabled = false;



        
        if (deathVFX != null)
        {


            deathVFX.transform.SetParent(null, worldPositionStays: true);

            deathVFX.Play(true);

         
            while (deathVFX.IsAlive(true))
                yield return null;
        }

         
        GameManager.Instance?.CompleteLevel(true);
        Destroy(gameObject);
    }
}
