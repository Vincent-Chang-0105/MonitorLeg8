using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public enum InteractionType{
        Click,
        Hold,
        Minigame
    }

    public InteractionType interactionType;
    public abstract string GetDescription();
    public abstract void Interact();
}
