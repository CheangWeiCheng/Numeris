/*
* Author: Kwek Sin En
* Date: 28/01/2026
* Description: Handles the logic for dropping loot items in the VR game. 
* When an enemy is defeated, this script determines which items to drop based on their drop chances and 
* instantiates them in the world with physics applied for a natural drop effect.
*/
using UnityEngine;
using System.Collections.Generic;

public class LootBag : MonoBehaviour
{
    [Header("Loot Settings")]
    public List<InventoryItem> lootList = new List<InventoryItem>();
    
    [Header("Drop Physics")]
    public float dropHeight = 1f;
    public float spreadRadius = 1f;

    /// <summary>
    /// Drops a loot item at a position above the current object if an item is available.
    /// </summary>
    public void DropLoot()
    {
        InventoryItem droppedItem = GetDroppedItem();
        if (droppedItem != null)
        {
            Vector3 dropPosition = transform.position + Vector3.up * dropHeight;
            InstantiateLoot(droppedItem, dropPosition);
        }
    }
    
    /// <summary>
    /// Selects and returns a random item from the loot list based on each item's drop chance.
    /// </summary>
    /// <returns>The dropped InventoryItem if any item passes the drop chance check; otherwise, null.</returns>
    InventoryItem GetDroppedItem()
    {
        // Roll a random number for each item
        List<InventoryItem> possibleItems = new List<InventoryItem>();
        foreach (InventoryItem item in lootList)
        {
            int roll = Random.Range(0, 101); // 0-100
            
            if (roll <= item.dropChance)
            {
                possibleItems.Add(item);
                Debug.Log($"Item {item.invenItemName} passed drop check! (Roll: {roll}, Chance: {item.dropChance})");
            }
        }
        if (possibleItems.Count > 0)
        {
            // Pick one random item from the possible drops
            InventoryItem droppedItem = possibleItems[Random.Range(0, possibleItems.Count)];
            Debug.Log($"Dropping: {droppedItem.invenItemName}");
            return droppedItem;
        }
        Debug.Log("No item dropped");
        return null;
    }

    /// <summary>
    /// Spawns a loot item in the world at the specified position with random spread and initializes its properties.
    /// </summary>
    /// <param name="item">The inventory item to instantiate as loot.</param>
    /// <param name="spawnPosition">The base position where the loot should be spawned.</param>
    void InstantiateLoot(InventoryItem item, Vector3 spawnPosition)
    {
        GameObject lootPrefab = item.lootPrefab3D;
        
        if (lootPrefab == null)
        {
            Debug.LogError($"No 3D loot prefab assigned for {item.invenItemName}!");
            return;
        }

        // Add random spread to drop position
        Vector3 randomOffset = new Vector3(
            Random.Range(-spreadRadius, spreadRadius),
            0f,
            Random.Range(-spreadRadius, spreadRadius)
        );
        
        Vector3 finalPosition = spawnPosition + randomOffset;
        
        // Instantiate the loot
        GameObject lootObject = Instantiate(lootPrefab, finalPosition, Quaternion.identity);
        
        // Setup the loot component
        LootItem lootComponent = lootObject.GetComponent<LootItem>();
        if (lootComponent == null)
        {
            lootComponent = lootObject.AddComponent<LootItem>();
        }
        lootComponent.InitializeLoot(item);
        
        // Apply physics if there's a Rigidbody
        Rigidbody rb = lootObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            //Add Rotation
            Vector3 randomTorque = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f)
            );
            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }
    }
}