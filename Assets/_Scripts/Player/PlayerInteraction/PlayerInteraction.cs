using System;
using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject interactionIcon;

    private Outline lastOutlinedObject;

    private void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        bool successfulHit = false;
        Outline newOutline = null; 

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            newOutline = hit.collider.GetComponent<Outline>();

            if (interactable != null)
            {
                HandleInteraction(interactable);
                interactionText.text = interactable.GetDescription();
                interactionIcon.SetActive(true);
                successfulHit = true;
            }
        }

        if (!successfulHit)
        {
            interactionText.text = "";
            interactionIcon.SetActive(false);
        }

        UpdateOutline(newOutline);
    }

    private void HandleInteraction(Interactable interactable)
    {
        switch (interactable.interactionType)
        {
            case Interactable.InteractionType.Click:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
                break;
            case Interactable.InteractionType.Hold:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
                break;
            case Interactable.InteractionType.Minigame:
                // Implement minigame logic here
                break;
            default:
                throw new System.Exception("Unsupported type of interactable");
        }
    }

    private void UpdateOutline(Outline newOutline)
    {
        if (lastOutlinedObject != null && lastOutlinedObject != newOutline)
        {
            lastOutlinedObject.enabled = false;
        }

        if (newOutline != null)
        {
            newOutline.enabled = true;
        }

        lastOutlinedObject = newOutline;
    }
}
