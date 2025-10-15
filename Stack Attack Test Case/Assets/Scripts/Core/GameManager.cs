using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoSingleton<GameManager>
{
    
     
    [SerializeField] private string[] levelSceneNames;

     
    [SerializeField] private int killsToUpgrade = 15;
    [SerializeField] private bool stopFiringatUpgrade = true;

    
    [SerializeField] private bool randomizeEnemyHP = true;

    
    [SerializeField] private Vector2Int enemyHpRange = new Vector2Int(40, 120);

     
    private int currentLevelIndex;
    private int currentKills;
    private bool isPlaying;

    private bool isLoading;

     
    private Scene? loadedLevelScene = null;
     
    [SerializeField] bool canControlPlayer = false;
    public bool CanControlPlayer => canControlPlayer;


    protected override void Awake()
    {
        base.Awake();
    }


    private void Start()
    {
        if (PlayerController.Instance == null)
        {
             
            return;
        }

        if (levelSceneNames == null || levelSceneNames.Length == 0)
        {
            
            return;
        }

        currentLevelIndex = SaveSystem.GetLastLevel(0);

        
        UIManager.Instance.ShowStart(currentLevelIndex + 1);
        UIManager.Instance.SetProgress(0f);
     
        SetPlaying(false);
        SetPlayerControl(false);


       
        FireController.Instance?.StopAll();

      
         
        var hurt = PlayerController.Instance.GetComponent<PlayerHurt>();
        if (hurt != null)
        {
            hurt.OnHeartsChanged.AddListener(UIManager.Instance.SetHearts);  
            hurt.OnDeath.AddListener(() => CompleteLevel(false));
            hurt.ResetFull();  
        }

        ResetKills(killsToUpgrade);
    }

    private void Update()
    {
        if (!isPlaying || PlayerController.Instance == null) return;
      
    }

   

    public void StartGameplay()
    {
        
        StartCoroutine(LoadLevelSceneCoroutine(currentLevelIndex, thenPlay: true));
    }

    public void RestartLevel()
    {
        FireController.Instance?.ResetAllWeaponsForNewRun();

        StartCoroutine(LoadLevelSceneCoroutine(currentLevelIndex, thenPlay: false));
        UIManager.Instance.ShowStart(currentLevelIndex + 1);

        SetPlaying(false);
        SetPlayerControl(false);     
        FireController.Instance?.StopAll();
        ResetKills(killsToUpgrade);
    }

    public void NextLevel()
    {
        FireController.Instance?.ResetAllWeaponsForNewRun();

        currentLevelIndex = (currentLevelIndex + 1) % levelSceneNames.Length;
        SaveSystem.SetLastLevel(currentLevelIndex);

        StartCoroutine(LoadLevelSceneCoroutine(currentLevelIndex, thenPlay: false));
        UIManager.Instance.ShowStart(currentLevelIndex + 1);

        SetPlaying(false);
        SetPlayerControl(false);     

        FireController.Instance?.StopAll();
        ResetKills(killsToUpgrade);
    }

    public void CompleteLevel(bool success)
    {
        SetPlaying(false);
        SetPlayerControl(false);     
        FireController.Instance?.StopAll();

        Projectile.DespawnAllActive();

        UIManager.Instance.ShowEnd(success);
      
        if (success)
        {
            int next = (currentLevelIndex + 1) % levelSceneNames.Length;
            SaveSystem.SetLastLevel(next);
        }

        
    }

     

    private IEnumerator LoadLevelSceneCoroutine(int index, bool thenPlay)
    {

        Projectile.DespawnAllActive();

         
        if (loadedLevelScene.HasValue && loadedLevelScene.Value.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelScene.Value);
            loadedLevelScene = null;
        }

        var sceneName = levelSceneNames[Mathf.Clamp(index, 0, levelSceneNames.Length - 1)];

       
        var sceneTemp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (sceneTemp == null)
        {
            Debug.LogError($"Sahne yüklenemedi: {sceneName} (Build Settings’e ekli mi?)");
            yield break;
        }
        while (!sceneTemp.isDone) yield return null;

       
        Scene lvl = SceneManager.GetSceneByName(sceneName);
        if (!lvl.IsValid() || !lvl.isLoaded)
        {
            Debug.LogError($"Sahne geçersiz/yüklenmedi: {sceneName}");
            yield break;
        }

        loadedLevelScene = lvl;
        SceneManager.SetActiveScene(lvl);

        
        PlacePlayerAtStart(lvl);
 
        UIManager.Instance.SetProgress(0f);
        var hurt = PlayerController.Instance.GetComponent<PlayerHurt>();
        if (hurt != null) hurt.ResetFull();

        if (thenPlay)
        {
            SetPlaying(true);
            SetPlayerControl(true);                
            UIManager.Instance.ShowHUD();
            FireController.Instance?.StartAll();
        }
    }

    private void PlacePlayerAtStart(Scene lvl)
    {
        if (!PlayerController.Instance) return;
        var p = PlayerController.Instance.transform.position;
        p.x = 0f;
        p.y = 0.5f;
        p.z = 4f;         
        PlayerController.Instance.transform.position = p;

        
        var rb = PlayerController.Instance.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezePositionZ |
                             RigidbodyConstraints.FreezeRotation;
        }
    }

    public void ResetProgressAndStartFromLevel1()
    {
      
        SaveSystem.ResetAll();

        currentLevelIndex = 0;
        StartCoroutine(LoadLevelSceneCoroutine(0, thenPlay: true));
    }







    public void RegisterKill()
    {
        currentKills++;
        if (currentKills >= killsToUpgrade)
            EnterUpgradeState();
    }

    public void ResetKills(int newGoal)
    {
        currentKills = 0;
        if (newGoal > 0) killsToUpgrade = newGoal;
     
    }

    private void EnterUpgradeState()
    {
        Time.timeScale = 0f;

     
        SetPlayerControl(false);

   
        if (stopFiringatUpgrade)
            FireController.Instance?.StopAll();
        else
            FireController.Instance?.SetHoldFireAll(true);

     
        UIManager.Instance?.ShowUpgradePanel();
    }

    public void ExitUpgradeStateAndContinue()
    {
        ResetKills(killsToUpgrade);
        UIManager.Instance?.HideUpgradePanel();
        Time.timeScale = 1f;

        SetPlayerControl(true);                 
        FireController.Instance?.StartAll();
        SetPlaying(true);
        SetPlayerControl(true);

        
        FireController.Instance?.SetHoldFireAll(false);

       
        FireController.Instance?.RestartAllFiring();

      

         

       
    }


    public void SetPlayerControl(bool value)
    {
        canControlPlayer = value;

        
        if (!value && PlayerController.Instance != null)
            PlayerController.Instance.HardStopMovement();
    }
 

    private void SetPlaying(bool value)
    {
        isPlaying = value;
        Time.timeScale = value ? 1f : 0f;
    }


    public void GetEnemyHPRandomRange(out int min, out int max)
    {
        if (!randomizeEnemyHP)
        {
            
            min = Mathf.Max(1, enemyHpRange.y);
            max = min;
            return;
        }

        min = Mathf.Max(1, enemyHpRange.x);
        max = Mathf.Max(min, enemyHpRange.y);
    }

   
    private IEnumerator ClearLevelAndShowStart()
    {
        if (loadedLevelScene.HasValue && loadedLevelScene.Value.isLoaded)
        {
            yield return UnityEngine.SceneManagement.SceneManager
                .UnloadSceneAsync(loadedLevelScene.Value);
            loadedLevelScene = null;
        }

        
        

      
        var hurt = PlayerController.Instance?.GetComponent<PlayerHurt>();
        if (hurt != null) hurt.ResetFull();
    }
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
          
            SaveSystem.SetLastLevel(currentLevelIndex);
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
           
            SaveSystem.SetLastLevel(currentLevelIndex);
        }
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        SaveSystem.SetLastLevel(currentLevelIndex);
    }
    public void OnQuitPressed()
    {
        SaveSystem.SetLastLevel(currentLevelIndex);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
     
}
