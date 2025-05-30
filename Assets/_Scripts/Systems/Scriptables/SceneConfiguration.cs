using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneConfig", menuName = "Game/Scene Configuration")]
public class SceneConfiguration : ScriptableObject
{
    [System.Serializable]
    public class SceneSettings
    {
        public string sceneName;
        public AudioClip musicClip;
        public bool hideCursorAtStart;
    }

    [Header("Scene Settings")]
    public List<SceneSettings> sceneSettings = new();
}
