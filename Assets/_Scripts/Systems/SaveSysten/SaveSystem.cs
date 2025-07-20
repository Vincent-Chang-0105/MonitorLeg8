using UnityEngine;

public static class SaveSystem
{
    private const string LAST_LEVEL_KEY = "LastLevel";
    private const string HAS_SAVE_DATA_KEY = "HasSaveData";
    
    public static void SaveProgress(string levelName)
    {
        PlayerPrefs.SetString(LAST_LEVEL_KEY, levelName);
        PlayerPrefs.SetInt(HAS_SAVE_DATA_KEY, 1);
        PlayerPrefs.Save();
        
        Debug.Log($"Progress saved: {levelName}");
    }
    
    public static string LoadLastLevel()
    {
        return PlayerPrefs.GetString(LAST_LEVEL_KEY, "Level1"); // Default to Level1
    }
    
    public static bool HasSaveData()
    {
        return PlayerPrefs.GetInt(HAS_SAVE_DATA_KEY, 0) == 1;
    }
    
    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(LAST_LEVEL_KEY);
        PlayerPrefs.DeleteKey(HAS_SAVE_DATA_KEY);
        PlayerPrefs.Save();
        
        Debug.Log("Save data cleared");
    }
}