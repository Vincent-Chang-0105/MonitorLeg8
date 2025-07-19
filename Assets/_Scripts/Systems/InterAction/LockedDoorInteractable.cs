using UnityEngine;
using AudioSystem;

public class LockedDoorInteractable : Interactable
{   
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Header("Key Requirements")]
    [SerializeField] private int requiredKeyID = 1;
    [SerializeField] private bool consumeKeyOnUse = false;
    
    [Header("Door Reference")]
    [SerializeField] private DoorController doorController; // Reference to shared door controller
    
    [Header("Sounds")]
    [SerializeField] SoundData interactButtonClick;
    [SerializeField] SoundData doorLockedSound;
    private SoundBuilder soundBuilder;
    
    public override void Interact()
    {
        // Check if player has the required key
        if (Inventory.Instance.HasItem(requiredKeyID))
        {
            soundBuilder.WithPosition(gameObject.transform.position).Play(interactButtonClick);
            
            // Use the shared door controller
            if (doorController != null)
            {
                doorController.ToggleDoor();
                
                // Optionally consume the key
                if (consumeKeyOnUse)
                {
                    Inventory.Instance.RemoveItem(requiredKeyID);
                }
            }
        }
        else
        {
            soundBuilder.WithPosition(gameObject.transform.position).PlaySequence(interactButtonClick, doorLockedSound);
            DialogueManager.Instance.StartDialogue(dialogue);
            TriggerHints();
            Debug.Log($"Door is locked! You need key ID: {requiredKeyID}");
        }
    }
    
    void Start()
    {
        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }
}