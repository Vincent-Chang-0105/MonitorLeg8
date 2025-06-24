using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : PersistentSingleton<Inventory>
{
    private HashSet<int> ownedItems = new HashSet<int>();

    public void AddItem(int itemID)
    {
        ownedItems.Add(itemID);
        Debug.Log($"Added item with ID: {itemID}");
    }
    
    public bool HasItem(int itemID)
    {
        return ownedItems.Contains(itemID);
    }
    
    public void RemoveItem(int itemID)
    {
        if (ownedItems.Remove(itemID))
        {
            Debug.Log($"Removed item with ID: {itemID}");
        }
    }
}
