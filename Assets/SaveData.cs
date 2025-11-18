using System;
using System.Collections.Generic;

[Serializable]
public class LevelResult
{
    public string sceneName;
    public float bestTime;   // for later, if you time levels
}

[Serializable]
public class SaveData
{
    // Last unlocked / current level
    public string lastScene;

    // Character + control selection (needed for spawning)
    public string p1Character;
    public string p1Scheme;
    public string p2Character;
    public string p2Scheme;

    // For future "Level Select" menu
    public List<LevelResult> levelResults = new List<LevelResult>();
}
