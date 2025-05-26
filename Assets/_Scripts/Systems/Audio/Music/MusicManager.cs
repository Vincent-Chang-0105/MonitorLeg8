using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem {
    public class MusicManager : PersistentSingleton<MusicManager> {
        const float crossFadeTime = 1.0f;
        float fading;
        
        // Use two persistent AudioSources instead of creating/destroying
        AudioSource audioSourceA;
        AudioSource audioSourceB;
        AudioSource current;
        AudioSource previous;
        
        readonly Queue<AudioClip> playlist = new();

        [SerializeField] List<AudioClip> initialPlaylist;
        [SerializeField] AudioMixerGroup musicMixerGroup;

        protected override void Awake() {
            base.Awake();
            
            // Create two persistent AudioSources
            audioSourceA = gameObject.AddComponent<AudioSource>();
            audioSourceB = gameObject.AddComponent<AudioSource>();
            
            // Configure both AudioSources
            SetupAudioSource(audioSourceA);
            SetupAudioSource(audioSourceB);
            
            current = audioSourceA;
        }

        void SetupAudioSource(AudioSource source) {
            source.outputAudioMixerGroup = musicMixerGroup;
            source.loop = false;
            source.volume = 0;
            source.bypassListenerEffects = true;
            source.playOnAwake = false;
        }

        void Start() {
            foreach (var clip in initialPlaylist) {
                AddToPlaylist(clip);
            }
        }

        public void AddToPlaylist(AudioClip clip) {
            playlist.Enqueue(clip);
            if (current == null && previous == null) {
                PlayNextTrack();
            }
        }

        public void Clear() => playlist.Clear();

        public void PlayNextTrack() {
            if (playlist.TryDequeue(out AudioClip nextTrack)) {
                Play(nextTrack);
            }
        }

        public void Play(AudioClip clip) {
            if (current && current.clip == clip) return;

            // Swap the AudioSources
            previous = current;
            current = (current == audioSourceA) ? audioSourceB : audioSourceA;

            // Setup the new current AudioSource
            current.clip = clip;
            current.volume = 0;
            current.Play();

            fading = 0.001f;
        }

        void Update() {
            HandleCrossFade();

            // Check if current track finished and play next
            // if (current && !current.isPlaying && playlist.Count > 0) {
            //     PlayNextTrack();
            // }
        }

        void HandleCrossFade() {
            if (fading <= 0f) return;
            
            fading += Time.deltaTime;

            float fraction = Mathf.Clamp01(fading / crossFadeTime);

            // Logarithmic fade
            float logFraction = fraction.ToLogarithmicFraction();

            if (previous) previous.volume = 1.0f - logFraction;
            if (current) current.volume = logFraction;

            if (fraction >= 1) {
                fading = 0.0f;
                if (previous) {
                    previous.Stop(); // Stop the previous track instead of destroying
                    previous.clip = null; // Clear the clip reference
                }
                previous = null;
            }
        }
    }
}