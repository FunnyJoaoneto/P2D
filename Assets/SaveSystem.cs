using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string path = Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.Save();
    }

    public static SaveData Load()
    {
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool SaveExists()
    {
        return PlayerPrefs.GetInt("HasSave", 0) == 1 && File.Exists(path);
    }

    public static void DeleteSave()
    {
        if (File.Exists(path))
            File.Delete(path);

        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.Save();
    }
}
