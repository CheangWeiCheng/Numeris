/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Manages the player's inventory in the VR game, 
* allowing for adding, using, and removing items.
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InvenManager : MonoBehaviour
{
    public static InvenManager instance;
    public List<InventoryItem> invenItemList = new List<InventoryItem>();

    public Transform invenItemContent;
    public GameObject invenItemPrefab;

    private bool isLoadingFromFirebase = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adds an inventory item, increasing quantity if it already exists or adding it as new.
    /// </summary>
    /// <param name="itemToAdd">The inventory item to add.</param>
    public void AddItem(InventoryItem itemToAdd)
    {
        if (itemToAdd == null)
        {
            Debug.LogError("Cannot add null item to inventory!");
            return;
        }

        InventoryItem existingItem = FindItemById(itemToAdd.invenId);
        AudioManager.Instance.PlayAddItem();
        if (existingItem != null)
        {
            existingItem.invenQuantity += itemToAdd.invenQuantity;
            Debug.Log($"Increased quantity of: {existingItem.invenItemName} to {existingItem.invenQuantity}");
        }
        else
        {
            InventoryItem runtimeItem = CreateRuntimeItem(itemToAdd, itemToAdd.invenQuantity);
            invenItemList.Add(runtimeItem);
            Debug.Log($"Added new item: {runtimeItem.invenItemName} (ID: {runtimeItem.invenId})");
        }

        RefreshInventory();
    }

    /// <summary>
    /// Uses the specified inventory item by applying its effect and removing one unit from the inventory.
    /// </summary>
    /// <param name="itemId">The unique identifier of the inventory item to use.</param>
    public void UseItem(int itemId)
    {
        Debug.Log($"=== UseItem called with ID: {itemId} ===");

        InventoryItem existingItem = FindItemById(itemId);

        if (existingItem == null)
        {
            Debug.LogError($"Item with ID {itemId} NOT FOUND in inventory!");
            DebugInventory();
            return;
        }

        Debug.Log($"Found item: {existingItem.invenItemName}, Quantity: {existingItem.invenQuantity}, PowerUpType: {existingItem.powerUpType}");

        ApplyItemEffect(existingItem);
        RemoveItem(itemId, 1);

        Debug.Log($"Used item: {existingItem.invenItemName} (ID: {itemId})");
    }

    /// <summary>
    /// Removes a specified quantity of an item from the inventory by its ID. If the quantity reaches zero or below, the
    /// item is removed entirely.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to remove.</param>
    /// <param name="quantity">The number of items to remove. Defaults to 1.</param>
    public void RemoveItem(int itemId, int quantity = 1)
    {
        InventoryItem existingItem = FindItemById(itemId);

        if (existingItem == null)
        {
            Debug.LogWarning($"Attempted to remove item with ID {itemId} but it wasn't found");
            return;
        }

        existingItem.invenQuantity -= quantity;
        Debug.Log($"Removed {quantity} of {existingItem.invenItemName}. Remaining: {existingItem.invenQuantity}");

        if (existingItem.invenQuantity <= 0)
        {
            invenItemList.Remove(existingItem);
            Debug.Log($"Item {existingItem.invenItemName} completely removed from inventory");
        }

        RefreshInventory();
    }

    /// <summary>
    /// Searches the inventory item list for an item with the specified ID.
    /// </summary>
    /// <param name="itemId">The unique identifier of the inventory item to find.</param>
    /// <returns>The inventory item with the matching ID, or null if not found.</returns>
    private InventoryItem FindItemById(int itemId)
    {
        return invenItemList.Find(item => item.invenId == itemId);
    }

    /// <summary>
    /// Creates a new InventoryItem instance at runtime by copying properties from a source item and setting its
    /// quantity.
    /// </summary>
    /// <param name="sourceItem">The InventoryItem to copy properties from.</param>
    /// <param name="quantity">The quantity to assign to the new InventoryItem.</param>
    /// <returns>A new InventoryItem instance with copied properties and specified quantity.</returns>
    private InventoryItem CreateRuntimeItem(InventoryItem sourceItem, int quantity)
    {
        InventoryItem runtimeItem = ScriptableObject.CreateInstance<InventoryItem>();
        runtimeItem.invenId = sourceItem.invenId;
        runtimeItem.invenItemName = sourceItem.invenItemName;
        runtimeItem.invenIcon = sourceItem.invenIcon;
        runtimeItem.itemTier = sourceItem.itemTier;
        runtimeItem.powerUpType = sourceItem.powerUpType;
        runtimeItem.invenQuantity = quantity;
        return runtimeItem;
    }

    /// <summary>
    /// Applies the effect of the specified inventory item by invoking the corresponding power-up action and playing the
    /// use item sound.
    /// </summary>
    /// <param name="item">The inventory item whose effect is to be applied.</param>
    private void ApplyItemEffect(InventoryItem item)
    {
        if (PowerUpManager.Instance == null)
        {
            Debug.LogError("PowerUpManager.Instance is null! Make sure PowerUpManager exists in the scene.");
            return;
        }

        Debug.Log($"Applying effect for PowerUpType: {item.powerUpType}");

        switch (item.powerUpType)
        {
            case PowerUpType.HealthPotion:
                Debug.Log("Using Health Potion");
                PowerUpManager.Instance.UseHealth();
                break;

            case PowerUpType.FiftyFifty:
                Debug.Log("Using 50:50 Power-Up");
                PowerUpManager.Instance.UseFiftyFifty();
                break;

            case PowerUpType.SwitchQuestion:
                Debug.Log("Using Switch Question Power-Up");
                PowerUpManager.Instance.UseSwitchQuestion();
                break;

            case PowerUpType.None:
            default:
                Debug.LogWarning($"No power-up assigned for item: {item.invenItemName}");
                break;
        }
        AudioManager.Instance.PlayUseItem();
    }

    /// <summary>
    /// Displays the current inventory items in the UI by clearing existing entries and instantiating UI elements for
    /// each item in the inventory list.
    /// </summary>
    public void DisplayInventory()
    {
        Debug.Log($"=== DisplayInventory - Total items: {invenItemList.Count} ===");

        if (!ValidateUIReferences())
        {
            return;
        }
        // Clear Inventory UI
        foreach (Transform child in invenItemContent)
        {
            Destroy(child.gameObject);
        }
        // Populate Inventory UI
        foreach (InventoryItem invenItem in invenItemList)
        {
            if (invenItem == null)
            {
                Debug.LogError("Null item found in inventory list!");
                continue;
            }

            GameObject itemObj = Instantiate(invenItemPrefab);

            if (itemObj == null)
            {
                Debug.LogError("Failed to instantiate inventory item prefab!");
                continue;
            }

            itemObj.transform.SetParent(invenItemContent, false);
            SetupInventoryItemUI(itemObj, invenItem);
        }
        Debug.Log($"Successfully displayed {invenItemList.Count} inventory items");
    }

    /// <summary>
    /// Checks whether the required UI references are assigned and logs warnings or errors if they are missing.
    /// </summary>
    /// <returns>True if all required UI references are assigned; otherwise, false.</returns>
    private bool ValidateUIReferences()
    {
        if (invenItemContent == null)
        {
            Debug.LogWarning("invenItemContent is null - UI may not be loaded yet");
            return false;
        }
        if (invenItemPrefab == null)
        {
            Debug.LogError("invenItemPrefab is NULL! Assign it in the Inspector.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Configures the UI elements of an inventory item, including its name, icon, quantity, and use button
    /// functionality.
    /// </summary>
    /// <param name="itemObj">The GameObject representing the inventory item UI element.</param>
    /// <param name="invenItem">The InventoryItem containing data to display in the UI.</param>
    private void SetupInventoryItemUI(GameObject itemObj, InventoryItem invenItem)
    {
        if (itemObj == null || invenItem == null)
        {
            Debug.LogError("Null reference in SetupInventoryItemUI!");
            return;
        }
        // Setup UI Elements
        TextMeshProUGUI itemName = FindChildComponent<TextMeshProUGUI>(itemObj.transform, "InvenItemName");
        Image itemImage = FindChildComponent<Image>(itemObj.transform, "InvenImage");
        TextMeshProUGUI itemQuantity = FindChildComponent<TextMeshProUGUI>(itemObj.transform, "InvenItemQuantity");
        if (itemName != null)
        {
            itemName.text = invenItem.invenItemName;
        }
        if (itemImage != null)
        {
            if (invenItem.invenIcon != null)
            {
                itemImage.sprite = invenItem.invenIcon;
            }
            else
            {
                Debug.LogWarning($"Icon is null for item: {invenItem.invenItemName}");
            }
        }
        if (itemQuantity != null)
        {
            itemQuantity.text = invenItem.invenQuantity.ToString();
        }
        // Setup Use Button
        Button useButton = itemObj.GetComponent<Button>();
        if (useButton == null)
        {
            Debug.LogError("Button component not found on InvenItem prefab!");
            return;
        }
        int currentItemId = invenItem.invenId;
        string currentItemName = invenItem.invenItemName;
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() => {
            Debug.Log($"=== USE BUTTON CLICKED - ID: {currentItemId}, Name: {currentItemName} ===");
            UseItem(currentItemId);
        });
        Debug.Log($"Button setup complete for {currentItemName} (ID: {currentItemId})");
    }

    /// <summary>
    /// Finds a child transform by name under the specified parent and returns the component of type T attached to it.
    /// </summary>
    /// <typeparam name="T">The type of Component to retrieve from the child transform.</typeparam>
    /// <param name="parent">The parent transform to search under.</param>
    /// <param name="childName">The name of the child transform to find.</param>
    /// <returns>The component of type T attached to the found child transform, or null if not found.</returns>
    private T FindChildComponent<T>(Transform parent, string childName) where T : Component
    {
        Transform child = parent.Find(childName);
        if (child == null)
        {
            Debug.LogError($"{childName} not found in prefab! Check your prefab structure.");
            return null;
        }
        T component = child.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"{typeof(T).Name} component not found on {childName}!");
            return null;
        }
        return component;
    }

    /// <summary>
    /// Sets the reference to the inventory UI content transform.
    /// </summary>
    /// <param name="contentTransform">The transform representing the inventory UI content.</param>
    public void SetInventoryUIReference(Transform contentTransform)
    {
        invenItemContent = contentTransform;
        Debug.Log("Inventory UI reference set");
    }

    /// <summary>
    /// Opens the inventory UI, loads inventory data from Firebase if the inventory panel is found, and plays the
    /// inventory opening sound.
    /// </summary>
    public void OpenInventoryUI()
    {
        Debug.Log("Opening inventory - Loading from Firebase...");

        if (TryFindInventoryPanel())
        {
            LoadInventoryFromFirebase();
        }
        AudioManager.Instance.PlayOpenInventory();
    }

    /// <summary>
    /// Attempts to locate the InventoryPanel GameObject and its Content child in the scene, assigning the Content
    /// transform to invenItemContent if found.
    /// </summary>
    /// <returns>True if both the InventoryPanel and its Content child are found; otherwise, false.</returns>
    private bool TryFindInventoryPanel()
    {
        GameObject inventoryPanel = GameObject.Find("InventoryPanel");
        if (inventoryPanel == null)
        {
            Debug.LogWarning("InventoryPanel GameObject not found in scene");
            return false;
        }
        Transform content = inventoryPanel.transform.Find("Content");

        if (content == null)
        {
            Debug.LogError("Content child not found in InventoryPanel!");
            return false;
        }
        invenItemContent = content;
        Debug.Log("Found inventory content panel");
        return true;
    }

    /// <summary>
    /// Saves the current inventory to Firebase using the FirebaseInventoryManager.
    /// </summary>
    public void SaveInventoryToFirebase()
    {
        Debug.Log("SaveInventoryToFirebase called");
        if (FirebaseInventoryManager.Instance == null)
        {
            Debug.LogError("FirebaseInventoryManager.Instance is NULL!");
            return;
        }
        List<Inventory> inventoryList = ConvertToFirebaseFormat();
        FirebaseInventoryManager.Instance.SaveInventory(inventoryList,
            onSuccess: () => Debug.Log("Inventory saved to Firebase successfully"),
            onError: (error) => Debug.LogError($"Failed to save inventory: {error}")
        );
    }

    /// <summary>
    /// Converts the inventory item list to a list of Inventory objects formatted for Firebase storage.
    /// </summary>
    /// <returns>A list of Inventory objects representing the inventory items in Firebase-compatible format.</returns>
    private List<Inventory> ConvertToFirebaseFormat()
    {
        List<Inventory> inventoryList = new List<Inventory>();
        foreach (InventoryItem item in invenItemList)
        {
            if (item == null) continue;
            Collectible collectible = new Collectible(
                item.invenId.ToString(),
                item.invenItemName,
                item.itemTier.ToString(),
                item.invenQuantity
            );
            Inventory inventory = new Inventory(item.invenId.ToString());
            inventory.collectibleDetails = collectible;
            inventoryList.Add(inventory);
        }
        return inventoryList;
    }

    /// <summary>
    /// Initiates loading of the inventory data from Firebase, handling duplicate calls and errors.
    /// </summary>
    public void LoadInventoryFromFirebase()
    {
        Debug.Log("=== LoadInventoryFromFirebase called ===");
        if (isLoadingFromFirebase)
        {
            Debug.LogWarning("Already loading inventory, skipping duplicate call");
            return;
        }
        if (FirebaseInventoryManager.Instance == null)
        {
            Debug.LogError("FirebaseInventoryManager.Instance is NULL!");
            return;
        }
        isLoadingFromFirebase = true;
        FirebaseInventoryManager.Instance.LoadInventory(
            onSuccess: (inventoryList) => OnInventoryLoadSuccess(inventoryList),
            onError: (error) => OnInventoryLoadError(error)
        );
    }

    /// <summary>
    /// Handles successful loading of inventory data from Firebase, validates and processes each item, updates the
    /// inventory list, and refreshes the display.
    /// </summary>
    /// <param name="inventoryList">The list of inventory items retrieved from Firebase.</param>
    private void OnInventoryLoadSuccess(List<Inventory> inventoryList)
    {
        invenItemList.Clear();
        Debug.Log($"Received {inventoryList.Count} items from Firebase");
        foreach (Inventory inventory in inventoryList)
        {
            if (!ValidateFirebaseInventoryItem(inventory))
            {
                continue;
            }
            LoadInventoryItemFromFirebase(inventory);
        }
        Debug.Log($"Loaded {invenItemList.Count} items from Firebase");
        DebugInventory();
        DisplayInventory();
        isLoadingFromFirebase = false;
    }

    /// <summary>
    /// Handles inventory load errors by logging the error message and updating the loading state.
    /// </summary>
    /// <param name="error">The error message describing the inventory load failure.</param>
    private void OnInventoryLoadError(string error)
    {
        Debug.LogError($"Failed to load inventory: {error}");
        isLoadingFromFirebase = false;
    }

    /// <summary>
    /// Checks whether the provided inventory and its collectible details are not null.
    /// </summary>
    /// <param name="inventory">The inventory object to validate.</param>
    /// <returns>True if the inventory and its collectible details are not null; otherwise, false.</returns>
    private bool ValidateFirebaseInventoryItem(Inventory inventory)
    {
        if (inventory == null || inventory.collectibleDetails == null)
        {
            Debug.LogWarning("Null inventory or collectible details, skipping");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Loads an inventory item from Firebase data and adds it to the inventory item list if valid.
    /// </summary>
    /// <param name="inventory">The inventory object containing collectible details to load.</param>
    private void LoadInventoryItemFromFirebase(Inventory inventory)
    {
        Collectible collectible = inventory.collectibleDetails;
        if (!int.TryParse(collectible.collectibleID, out int itemId))
        {
            Debug.LogWarning($"Invalid collectible ID: {collectible.collectibleID}");
            return;
        }
        InventoryItem itemAsset = GetInventoryItemAsset(itemId);
        if (itemAsset != null)
        {
            InventoryItem runtimeItem = CreateRuntimeItem(itemAsset, collectible.quantity);
            invenItemList.Add(runtimeItem);
            Debug.Log($"Loaded item: {runtimeItem.invenItemName} (ID: {runtimeItem.invenId}, PowerUp: {runtimeItem.powerUpType})");
        }
        else
        {
            Debug.LogWarning($"Could not find inventory item asset with ID: {collectible.collectibleID}");
        }
    }

    /// <summary>
    /// Retrieves an InventoryItem asset from the Resources/InventoryItems folder by its unique ID.
    /// </summary>
    /// <param name="itemId">The unique identifier of the inventory item to retrieve.</param>
    /// <returns>The InventoryItem asset matching the specified ID, or null if not found.</returns>
    private InventoryItem GetInventoryItemAsset(int itemId)
    {
        InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("InventoryItems");
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogError("No InventoryItems found in Resources/InventoryItems folder!");
            return null;
        }
        foreach (InventoryItem item in allItems)
        {
            if (item != null && item.invenId == itemId)
            {
                return item;
            }
        }
        Debug.LogWarning($"No InventoryItem found with ID {itemId}. Available IDs: {string.Join(", ", Array.ConvertAll(allItems, i => i.invenId.ToString()))}");
        return null;
    }

    /// <summary>
    /// Updates the displayed inventory and saves the current inventory state to Firebase.
    /// </summary>
    private void RefreshInventory()
    {
        DisplayInventory();
        SaveInventoryToFirebase();
    }

    /// <summary>
    /// Logs the current inventory contents and details of each item to the debug console.
    /// </summary>
    public void DebugInventory()
    {
        Debug.Log("=== CURRENT INVENTORY ===");
        if (invenItemList.Count == 0)
        {
            Debug.Log("Inventory is EMPTY!");
        }
        else
        {
            foreach (var item in invenItemList)
            {
                if (item != null)
                {
                    Debug.Log($"ID: {item.invenId} | Name: {item.invenItemName} | Quantity: {item.invenQuantity} | PowerUp: {item.powerUpType}");
                }
                else
                {
                    Debug.LogError("NULL ITEM in inventory list!");
                }
            }
        }
        Debug.Log("========================");
    }
}