using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicConfig", menuName = "Audio/Music Configuration")]
public class MusicConfiguration : ScriptableObject
{
    [System.Serializable]
    public class SceneMusicPair
    {
        public string sceneName;
        public AudioClip musicClip;
    }
    
    [System.Serializable]
    public class EventMusicPair
    {
        public string eventName;
        public AudioClip musicClip;
        public int defaultPriority = 1;
    }
    
    [Header("Scene Music")]
    [SerializeField] public List<SceneMusicPair> sceneMusicMappings = new List<SceneMusicPair>();
    
    [Header("Event Music")]
    [SerializeField] public List<EventMusicPair> eventMusicMappings = new List<EventMusicPair>();
    
    [Header("Default Music")]
    [SerializeField] public AudioClip defaultMusic;
}
