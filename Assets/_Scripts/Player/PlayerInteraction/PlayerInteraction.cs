using System;
using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private TextMeshProUGUI interactionName;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject interactionIcon;
    [SerializeField] private LayerMask interactionLayerMask = -1;

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
                interactionName.text = interactable.GetName();
                interactionIcon.SetActive(true);

                // ➕ NEW: Make interactionName follow the object in screen space
                Vector3 screenPosition = playerCamera.WorldToScreenPoint(hit.collider.bounds.center);

                // Optional: Check if it's in front of the camera
                if (screenPosition.z > 0)
                {
                    interactionIcon.GetComponent<RectTransform>().position = new Vector3(screenPosition.x, screenPosition.y, 0);
                    interactionName.rectTransform.position = new Vector3(screenPosition.x, screenPosition.y + 70f, 0); // optional offset
                }
                else
                {
                    // Object is behind the camera
                    interactionName.text = "";
                }

                successfulHit = true;
            }
        }



        if (!successfulHit)
        {
            interactionText.text = "";
            interactionName.text = "";
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
    
    private void OnDisable()
    {
        interactionText.text = "";
        interactionName.text = "";
        interactionIcon.SetActive(false);
    }
}
