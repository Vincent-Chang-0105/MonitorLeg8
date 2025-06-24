using DG.Tweening; // Required for DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

public class PickableInteractable : Interactable
{
    [SerializeField] private Sprite itemIcon; // Icon for inventory display
    [SerializeField] private int itemID = 0; // Unique identifier for the item
    [SerializeField] private int quantity = 1; // How many of this item to give

    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Header("Pickup Animation")]
    [SerializeField] private bool usePickupAnimation = true;
    [SerializeField] private float pickupDuration = 0.5f;
    [SerializeField] private float floatHeight = 1f; // How high the object floats before disappearing
    [SerializeField] private Ease pickupEase = Ease.OutQuad;
    [SerializeField] private bool fadeOut = true; // Should the object fade out during pickup

    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupVFX; // Optional particle effect on pickup
    [SerializeField] private bool destroyAfterPickup = true; // Should the object be destroyed after pickup

    [Header("Sounds")]
    [SerializeField] SoundData interactPickupObject;
    private SoundBuilder soundBuilder;

    private Renderer objectRenderer;
    private Collider objectCollider;

    private void PickupObject()
    {
        // Add item to inventory (you'll need to implement your inventory system)
        AddToInventory();

        // Play pickup sound
        soundBuilder.Play(interactPickupObject);

        // Spawn pickup VFX if assigned
        if (pickupVFX != null)
        {
            Instantiate(pickupVFX, transform.position, transform.rotation);
        }

        // Disable collider to prevent further interactions
        if (objectCollider != null)
            objectCollider.enabled = false;

        // Play pickup animation
        if (usePickupAnimation)
        {
            PlayPickupAnimation();
        }
        else
        {
            // Destroy immediately if no animation
            if (destroyAfterPickup)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    private void PlayPickupAnimation()
    {
        Vector3 targetPosition = transform.position + Vector3.up * floatHeight;
        
        // Create animation sequence
        Sequence pickupSequence = DOTween.Sequence();
        
        // Move up and optionally fade out
        pickupSequence.Append(transform.DOMove(targetPosition, pickupDuration).SetEase(pickupEase));
        
        if (fadeOut && objectRenderer != null)
        {
            // Fade out during the movement
            if (objectRenderer.material.HasProperty("_Color"))
            {
                pickupSequence.Join(objectRenderer.material.DOFade(0f, pickupDuration));
            }
            else if (objectRenderer.material.HasProperty("_BaseColor")) // URP/HDRP
            {
                pickupSequence.Join(objectRenderer.material.DOColor(new Color(1, 1, 1, 0), "_BaseColor", pickupDuration));
            }
        }
        
        // Scale down effect
        pickupSequence.Join(transform.DOScale(Vector3.zero, pickupDuration * 0.8f).SetDelay(pickupDuration * 0.2f));
        
        // Destroy or deactivate when animation completes
        pickupSequence.OnComplete(() =>
        {
            if (destroyAfterPickup)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        });
    }

    private void AddToInventory()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(itemID);
        }
        else
        {
            Debug.LogError("SimpleInventory not found!");
        }
    }

    public override void Interact()
    {
        PickupObject();
        TriggerHints();
    }

    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();
        
        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<Renderer>();
        
        if (objectCollider == null)
            objectCollider = GetComponentInChildren<Collider>();

        soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        
    }

    // Optional: Visualize pickup area in editor
    private void OnDrawGizmosSelected()
    {
        // Draw pickup indicator
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        // Draw pickup animation preview
        if (usePickupAnimation)
        {
            Gizmos.color = Color.yellow;
            Vector3 targetPos = transform.position + Vector3.up * floatHeight;
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.DrawWireSphere(targetPos, 0.1f);
        }
        
        // Draw quantity indicator if more than 1
        if (quantity > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < Mathf.Min(quantity, 5); i++)
            {
                Vector3 offset = new Vector3(i * 0.1f, 0, 0);
                Gizmos.DrawWireCube(transform.position + offset, Vector3.one * 0.05f);
            }
        }
    }
}