using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "SceneConfig", menuName = "Game/Scene Configuration")]
public class SceneConfiguration : ScriptableObject
{
    [System.Serializable]
    public class SceneSettings
    {
        public string sceneName;
        public AudioClip musicClip;
        public bool hideCursorAtStart;
        public HintData hintData;
        public VideoClip introVideoClip;
        public VideoClip outroVideoClip;
    }

    [Header("Scene Settings")]
    public List<SceneSettings> sceneSettings = new();
}
