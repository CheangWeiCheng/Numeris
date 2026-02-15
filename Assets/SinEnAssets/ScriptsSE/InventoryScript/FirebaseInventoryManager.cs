/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: 
*/
using UnityEngine;
using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System.Threading.Tasks;

public class FirebaseInventoryManager : MonoBehaviour
{
    public static FirebaseInventoryManager Instance;
    private DatabaseReference db;
    private FirebaseAuth auth;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseDatabase.DefaultInstance.RootReference;
    }

    /// <summary>
    /// Determines whether the current user is authenticated.
    /// </summary>
    /// <returns>true if the current user exists and has a non-empty user ID; otherwise, false.</returns>
    private bool IsAuthenticated() =>
        auth.CurrentUser != null &&
        !string.IsNullOrEmpty(auth.CurrentUser.UserId);

    /// <summary>
    /// Retrieves the user ID of the currently authenticated user.
    /// </summary>
    /// <returns>The user ID of the current user.</returns>
    private string CurrentUserId() => auth.CurrentUser.UserId;
  

    /// <summary>
    /// Saves the specified inventory list for the authenticated user, handling success and error callbacks.
    /// </summary>
    /// <param name="inventoryList">The list of inventory items to be saved.</param>
    /// <param name="onSuccess">Callback invoked when the inventory is saved successfully.</param>
    /// <param name="onError">Callback invoked with an error message if saving fails.</param>
    public void SaveInventory(List<Inventory> inventoryList, Action onSuccess, Action<string> onError)
    {
        Debug.Log("SaveInventory method called");
        if (!IsAuthenticated())
        {
            HandleError("User not authenticated", onError);
            return;
        }
        string userId = CurrentUserId();
        Debug.Log($"Saving inventory for user: {userId}");
        ClearExistingInventory(userId, inventoryList, onSuccess, onError);
    }

    /// <summary>
    /// Clears the existing inventory for a user and saves a new list of inventory items.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose inventory is being updated.</param>
    /// <param name="inventoryList">The list of new inventory items to be saved.</param>
    /// <param name="onSuccess">Callback invoked upon successful completion.</param>
    /// <param name="onError">Callback invoked if an error occurs, with the error message.</param>
    private void ClearExistingInventory(string userId, List<Inventory> inventoryList, Action onSuccess, Action<string> onError)
    {
        db.Child("players").Child(userId).Child("inventoryItems").RemoveValueAsync()
            .ContinueWithOnMainThread(clearTask =>
            {
                if (HandleTaskFailure(clearTask, "Clear", onError))
                {
                    return;
                }
                Debug.Log("Old inventory cleared, now saving new items...");
                SaveNewInventoryItems(userId, inventoryList, onSuccess, onError);
            });
    }

    /// <summary>
    /// Saves a list of new inventory items for a user to Firebase and invokes callbacks based on the operation result.
    /// </summary>
    /// <param name="userId">The user ID associated with the inventory items.</param>
    /// <param name="inventoryList">The list of inventory items to be saved.</param>
    /// <param name="onSuccess">Callback invoked when the save operation completes successfully.</param>
    /// <param name="onError">Callback invoked with an error message if the save operation fails.</param>
    private void SaveNewInventoryItems(string userId, List<Inventory> inventoryList, Action onSuccess, Action<string> onError)
    {
        if (inventoryList.Count == 0)
        {
            Debug.Log("No items to save");
            onSuccess();
            return;
        }
        Dictionary<string, object> updates = BuildInventoryUpdates(userId, inventoryList);
        Debug.Log($"Updating {updates.Count} items in Firebase");

        db.UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(updateTask =>
            {
                if (HandleTaskFailure(updateTask, "Update", onError))
                {
                    return;
                }
                Debug.Log("Inventory saved successfully to Firebase!");
                onSuccess();
            });
    }

    /// <summary>
    /// Creates a dictionary of inventory updates for a user, mapping inventory item paths to their corresponding data.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose inventory is being updated.</param>
    /// <param name="inventoryList">The list of inventory items to include in the updates.</param>
    /// <returns>A dictionary containing inventory update paths as keys and item data as values.</returns>
    private Dictionary<string, object> BuildInventoryUpdates(string userId, List<Inventory> inventoryList)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>();
        for (int i = 0; i < inventoryList.Count; i++)
        {
            string json = JsonUtility.ToJson(inventoryList[i]);
            Debug.Log($"Item {i} JSON: {json}");

            Dictionary<string, object> itemData = CreateItemData(inventoryList[i]);
            updates[$"/players/{userId}/inventoryItems/item_{i}"] = itemData;
        }
        return updates;
    }

    /// <summary>
    /// Creates a dictionary containing item and collectible details from the specified inventory.
    /// </summary>
    /// <param name="inventory">The inventory object to extract item data from.</param>
    /// <returns>A dictionary with item ID and nested collectible details.</returns>
    private Dictionary<string, object> CreateItemData(Inventory inventory)
    {
        return new Dictionary<string, object>
        {
            { "itemID", inventory.itemID },
            { "collectibleDetails", new Dictionary<string, object>
                {
                    { "collectibleID", inventory.collectibleDetails.collectibleID },
                    { "collectibleName", inventory.collectibleDetails.collectibleName },
                    { "tier", inventory.collectibleDetails.tier },
                    { "quantity", inventory.collectibleDetails.quantity }
                }
            }
        };
    }

    /// <summary>
    /// Loads the current user's inventory from the database and returns the result asynchronously.
    /// </summary>
    /// <param name="onSuccess">Callback invoked with the list of inventory items on successful load.</param>
    /// <param name="onError">Callback invoked with an error message if loading fails.</param>
    public void LoadInventory(Action<List<Inventory>> onSuccess, Action<string> onError)
    {
        Debug.Log("LoadInventory method called");
        if (!IsAuthenticated())
        {
            HandleError("User not authenticated", onError);
            return;
        }
        string userId = CurrentUserId();
        db.Child("players").Child(userId).Child("inventoryItems").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (HandleTaskFailure(task, "Load", onError))
                {
                    return;
                }
                if (task.IsCompleted)
                {
                    List<Inventory> inventoryList = ParseInventoryFromSnapshot(task.Result);
                    Debug.Log($"Inventory loaded: {inventoryList.Count} items");
                    onSuccess(inventoryList);
                }
            });
    }

    /// <summary>
    /// Parses a DataSnapshot and returns a list of Inventory objects represented in the snapshot.
    /// </summary>
    /// <param name="snapshot">The DataSnapshot containing inventory data to parse.</param>
    /// <returns>A list of Inventory objects parsed from the snapshot, or an empty list if the snapshot does not exist.</returns>
    private List<Inventory> ParseInventoryFromSnapshot(DataSnapshot snapshot)
    {
        List<Inventory> inventoryList = new List<Inventory>();
        if (!snapshot.Exists)
        {
            return inventoryList;
        }
        foreach (DataSnapshot childSnapshot in snapshot.Children)
        {
            string json = childSnapshot.GetRawJsonValue();
            Inventory inventory = JsonUtility.FromJson<Inventory>(json);
            inventoryList.Add(inventory);
        }
        return inventoryList;
    }
    
    /// <summary>
    /// Handles a failed or canceled task by reporting an error and indicating whether a failure occurred.
    /// </summary>
    /// <param name="task">The task to check for failure or cancellation.</param>
    /// <param name="operationType">A string describing the type of operation associated with the task.</param>
    /// <param name="onError">An action to invoke with the error message if a failure is detected.</param>
    /// <returns>True if the task was canceled or faulted; otherwise, false.</returns>
    private bool HandleTaskFailure(Task task, string operationType, Action<string> onError)
    {
        if (task.IsCanceled)
        {
            HandleError($"{operationType} task canceled", onError);
            return true;
        }
        if (task.IsFaulted)
        {
            string errorMessage = task.Exception?.ToString() ?? "Unknown error";
            HandleError(errorMessage, onError);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Logs the specified error and invokes the provided error handler.
    /// </summary>
    /// <param name="error">The error message to log and handle.</param>
    /// <param name="onError">The action to invoke with the error message.</param>
    private void HandleError(string error, Action<string> onError)
    {
        Debug.LogError(error);
        onError(error);
    }
}