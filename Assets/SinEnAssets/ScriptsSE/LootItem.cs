/*
* Author: Kwek Sin En
* Date: 28/01/2026
* Description: Represents a loot item in the VR game world. 
* This script handles the visual representation of the loot, including optional rotation for better visibility, 
* and manages the logic for picking up the item and adding it to the player's inventory.
*/
using UnityEngine;

public class LootItem : MonoBehaviour
{
    private InventoryItem itemItem;
    public float pickupRadius = 1.5f;

    [Header("Visual Effects")]
    public bool rotateItem = true;
    public float rotationSpeed = 50f;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    /// <summary>
    /// Initializes the loot object with the specified inventory item and updates its name.
    /// </summary>
    /// <param name="item">The inventory item to associate with the loot object.</param>
    public void InitializeLoot(InventoryItem item)
    {
        itemItem = item;
        gameObject.name = $"Loot_{item.invenItemName}";
        
        Debug.Log($"Loot initialized: {item.invenItemName}");
    }

    /// <summary>
    /// Rotates the transform around the Y axis if rotation is enabled.
    /// </summary>
    private void Update()
    {
        if (rotateItem)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Adds the associated item to the inventory, logs the pickup, and destroys the game object.
    /// </summary>
    public void PickupItem()
    {
        if (itemItem == null)
        {
            Debug.LogError("No item data!");
            return;
        }
        InvenManager.instance.AddItem(itemItem);
        Debug.Log($"Picked up: {itemItem.invenItemName}");
        Destroy(gameObject);
    }
}
