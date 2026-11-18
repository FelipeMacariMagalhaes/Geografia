using UnityEngine;

public static class LevelProgress
{
    private const string Prefix = "level_done_";
    private const string FinalKey = "final_shown";

    public static bool IsCompleted(string scene)
    {
        return PlayerPrefs.GetInt(Prefix + scene, 0) == 1;
    }

    public static void MarkCompleted(string scene)
    {
        PlayerPrefs.SetInt(Prefix + scene, 1);
        PlayerPrefs.Save();
    }

    public static bool FinalShown
    {
        get => PlayerPrefs.GetInt(FinalKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(FinalKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static void ResetAll(string[] scenes)
    {
        foreach (string s in scenes)
            PlayerPrefs.DeleteKey(Prefix + s);

        PlayerPrefs.DeleteKey(FinalKey);
        PlayerPrefs.Save();
    }
}