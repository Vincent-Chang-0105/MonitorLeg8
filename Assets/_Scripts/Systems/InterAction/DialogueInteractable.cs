using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class DialogueInteractable : Interactable
{
    [SerializeField] private string objName;
    [SerializeField] private string objDescription;
    
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Header("Hint")]
    [SerializeField] private int hintIdToComplete;
    [SerializeField] private int hintIdToTrigger;
    private HintTrigger hintTrigger;

    [Header("Sounds")]
    [SerializeField] SoundData interactButtonClick;
    private SoundBuilder soundBuilder;

    public override string GetDescription()
    {
        return objDescription;
    }

    public override string GetName()
    {
        return objName;
    }

    public override void Interact()
    {
        // Play interaction sound
        soundBuilder.WithPosition(gameObject.transform.position).Play(interactButtonClick);

        // Start the dialogue
        if (dialogue != null && dialogue.dialogueLines.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("No dialogue assigned or dialogue is empty in " + gameObject.name);
        }

        // Trigger hints if component exists
        if (hintTrigger != null)
            hintTrigger.OnInteract();
    }

    void Start()
    {
        hintTrigger = GetComponent<HintTrigger>();
        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
    }
}