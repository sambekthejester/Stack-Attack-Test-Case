using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoSingleton<UIManager>
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject upgradePanel;

    [Header("Start Panel")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button tapToStartButton;
    [SerializeField] private Button resetAndStartButton;

    [Header("HUD Panel")]
    [SerializeField] private Slider progressBar;           
    [SerializeField] private Transform heartsContainer;   
    [SerializeField] private GameObject heartPrefab;     

    [Header("End Panel")]
    [SerializeField] private GameObject winGroup;
    [SerializeField] private GameObject loseGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextButton;

    [Header("Upgrade Panel")]
     
    [SerializeField] private UpgradeController upgradeController;
 
    private readonly List<GameObject> hearts = new List<GameObject>();
    private int spawnedMax = 0;

    protected override void Awake()
    {
        base.Awake();

        if (tapToStartButton) tapToStartButton.onClick.AddListener(() => GameManager.Instance.StartGameplay());
        if (restartButton) restartButton.onClick.AddListener(() => GameManager.Instance.RestartLevel());
        if (nextButton) nextButton.onClick.AddListener(() => GameManager.Instance.NextLevel());
        if (resetAndStartButton) resetAndStartButton.onClick.AddListener(() => GameManager.Instance.ResetProgressAndStartFromLevel1());
 
    }

 
    public void ShowStart(int levelNumber)
    {
        if (levelText) levelText.text = $"Level {levelNumber}";
        SetPanels(true, false, false, false);
    }
    public void ShowHUD() => SetPanels(false, true, false, false);
    public void ShowEnd(bool win)
    {
        Debug.Log(win);
        SetPanels(false, false, true, false);
        if (winGroup) winGroup.SetActive(win);
        if (loseGroup) loseGroup.SetActive(!win);
        if (nextButton) nextButton.gameObject.SetActive(win);

    }
    public void ShowUpgradePanel()
    {
   
        if (upgradeController) upgradeController.GenerateOptions();

        if (upgradePanel) upgradePanel.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
        if (startPanel) startPanel.SetActive(false);
        if (endPanel) endPanel.SetActive(false);
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel) upgradePanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(true);
    }

    private void SetPanels(bool start, bool hud, bool end, bool upgrade)
    {
        if (startPanel) startPanel.SetActive(start);
        if (hudPanel) hudPanel.SetActive(hud);
        if (endPanel) endPanel.SetActive(end);
        if (upgradePanel) upgradePanel.SetActive(upgrade);
    }

  
    public void SetProgress(float v) { if (progressBar) progressBar.value = Mathf.Clamp01(v); }

 
    public void SetHearts(int current, int max)
    {
        if (!heartsContainer || !heartPrefab) return;

        if (max != spawnedMax)
            RebuildHearts(max);

        current = Mathf.Clamp(current, 0, max);

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].SetActive(i < current);
    }

    private void RebuildHearts(int max)
    {
 
        for (int i = heartsContainer.childCount - 1; i >= 0; i--)
            Destroy(heartsContainer.GetChild(i).gameObject);
        hearts.Clear();

       
        for (int i = 0; i < max; i++)
        {
            var go = Instantiate(heartPrefab, heartsContainer);
             
            hearts.Add(go);
        }
        spawnedMax = max;
      
    }
}
