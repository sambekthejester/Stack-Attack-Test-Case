using UnityEngine;

public static class SaveSystem
{
    const string KEY_LEVEL = "LastLevelIndex";

    public static int GetLastLevel(int defaultLevel = 0)
        => PlayerPrefs.GetInt(KEY_LEVEL, defaultLevel);

    public static void SetLastLevel(int index)
    {
        PlayerPrefs.SetInt(KEY_LEVEL, index);
        PlayerPrefs.Save();
    }
    public static void ResetAll()
    {
       
        PlayerPrefs.SetInt(KEY_LEVEL, 0);
        PlayerPrefs.Save();
    }

}
