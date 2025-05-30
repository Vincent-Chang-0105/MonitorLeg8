using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuEventHandler : ColorChangeMenuHandler
{
    [Header("Camera Controller")]
    [SerializeField] private MainMenuCameraEventHandler cameraEventHandler;
    
    [Header("Menu Preset Mapping")]
    [SerializeField] private List<MenuCameraPreset> menuCameraPresets = new List<MenuCameraPreset>();

    [Header("Sounds")]
    [SerializeField] SoundData soundDataButtonHover;
    [SerializeField] SoundData soundDataButtonClick;    
    private SoundBuilder soundBuilder;

    [System.Serializable]
    public class MenuCameraPreset
    {
        public Selectable menuItem;
        public string cameraPresetName;
    }
    
    private Dictionary<Selectable, string> presetMappings = new Dictionary<Selectable, string>();
    
    public override void Awake()
    {
        base.Awake();
        
        // Find camera controller if not assigned
        if (cameraEventHandler == null)
        {
            cameraEventHandler = FindObjectOfType<MainMenuCameraEventHandler>();
            if (cameraEventHandler == null)
            {
                Debug.LogError("No MainMenuCameraEventHandler found in scene!");
            }
        }
        
        // Create mapping dictionary for quicker lookups
        foreach (var preset in menuCameraPresets)
        {
            if (preset.menuItem != null && !string.IsNullOrEmpty(preset.cameraPresetName))
            {
                presetMappings[preset.menuItem] = preset.cameraPresetName;
            }
        }

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }
    
    protected override void HandleSelect(Selectable selectable)
    {
        // Handle text color change from base class
        base.HandleSelect(selectable);
        
        // Make Sound
        soundBuilder.WithRandomPitch().Play(soundDataButtonHover);

        // Handle camera activation
        if (cameraEventHandler != null && presetMappings.ContainsKey(selectable))
        {
            cameraEventHandler.ActivatePreset(presetMappings[selectable]);
        }
    }

    protected override void HandleDeselect(Selectable selectable)
    {
        // Handle text color reset from base class
        base.HandleDeselect(selectable);
    }

    protected override void HandleClick(Selectable selectable)
    {
        base.HandleClick(selectable);

        // Make Sound
        soundBuilder.WithRandomPitch().Play(soundDataButtonClick);
    }
    // Add game functionality methods
    public void StartGame()
    {
        GameManager.Instance.LoadScene("Level1");
        Debug.Log("Starting new game...");
        // Add your game start logic here
    }
    
    public void ContinueGame()
    {
        Debug.Log("Continuing game...");
        // Add your continue game logic here
    }
    
    public void OpenSettings()
    {
        Debug.Log("Opening settings...");
        // Add your settings logic here
    }
    
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
