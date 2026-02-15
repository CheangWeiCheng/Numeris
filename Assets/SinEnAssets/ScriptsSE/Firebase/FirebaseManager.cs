/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Manages all interactions with Firebase for the VR game, 
* including player authentication, data retrieval and updates, inventory management, and leaderboard functionality.
*/
using System;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    private FirebaseAuth auth;
    private DatabaseReference db;
    private bool isFirebaseInitialized = false;
    public string DisplayName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes Firebase authentication and database references and sets the initialization flag.
    /// </summary>
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseDatabase.DefaultInstance.RootReference;
        isFirebaseInitialized = true;
        Debug.Log("Firebase initialized");
    }

    /// <summary>
    /// Determines whether the current user is authenticated based on the presence of a non-empty, non-null user ID.
    /// </summary>
    /// <returns>true if the current user's UserId is not null or empty; otherwise, false.</returns>
    private bool IsAuthenticated() =>
        auth.CurrentUser.UserId != "" &&
        auth.CurrentUser.UserId != null;

    /// <summary>
    /// Retrieves the user ID of the currently authenticated user.
    /// </summary>
    /// <returns>The user ID of the current user.</returns>
    private string CurrentUserId() =>
        auth.CurrentUser.UserId;

    /// <summary>
    /// Creates a new player with the specified username and email, saving the data to the database if the user is
    /// authenticated.
    /// </summary>
    /// <param name="username">The username for the new player.</param>
    /// <param name="email">The email address for the new player.</param>
    /// <param name="onError">Callback invoked with an error message if player creation fails.</param>
    /// <param name="onSuccess">Callback invoked if player creation succeeds.</param>
    public void CreatePlayer(string username, string email, Action<string> onError, Action onSuccess)
    {
        if (!IsAuthenticated())
        {
            Debug.Log("Cannot create player when user is not logged in!");
            onError("User not authenticated");
            return;
        }

        Player newPlayer = new Player(username, email);
        newPlayer.isLoggedIn = true;

        string json = JsonUtility.ToJson(newPlayer);
        string userId = CurrentUserId();

        db.Child("players").Child(userId).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }

                Debug.Log("Player created successfully!");
                onSuccess();
            });
    }

    /// <summary>
    /// Loads the current authenticated player's data from the database and invokes the appropriate callback based on
    /// the result.
    /// </summary>
    /// <param name="onSuccess">Callback invoked with the loaded Player object if retrieval is successful.</param>
    /// <param name="onError">Callback invoked with an error message if loading fails or the user is not authenticated.</param>
    public void LoadPlayer(Action<Player> onSuccess, Action<string> onError)
    {
        if (!IsAuthenticated())
        {
            Debug.Log("Cannot load player when user is not logged in!");
            onError("User not authenticated");
            return;
        }

        string userId = CurrentUserId();
        db.Child("players").Child(userId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }

                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();
                        Player player = JsonUtility.FromJson<Player>(json);
                        Debug.Log("Player loaded: " + player.username);
                        onSuccess(player);
                    }
                    else
                    {
                        onError("Player data not found");
                    }
                }
            });
    }

    /// <summary>
    /// Updates the current user's player data in the database and invokes callbacks based on the operation result.
    /// </summary>
    /// <param name="player">The player data to update.</param>
    /// <param name="onSuccess">Callback invoked if the update succeeds.</param>
    /// <param name="onError">Callback invoked with an error message if the update fails.</param>
    public void UpdatePlayer(Player player, Action onSuccess, Action<string> onError)
    {
        if (!IsAuthenticated())
        {
            Debug.Log("Cannot update player when user is not logged in!");
            onError("User not authenticated");
            return;
        }
        string json = JsonUtility.ToJson(player);
        string userId = CurrentUserId();

        db.Child("players").Child(userId).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }

                Debug.Log("Player updated successfully!");
                onSuccess();
            });
    }

    /// <summary>
    /// Updates a specified field for the current authenticated player in the database.
    /// </summary>
    /// <param name="fieldName">The name of the player field to update.</param>
    /// <param name="value">The new value to set for the specified field.</param>
    /// <param name="onSuccess">Callback invoked if the update is successful.</param>
    /// <param name="onError">Callback invoked with an error message if the update fails.</param>
    public void UpdatePlayerField(string fieldName, object value, Action onSuccess, Action<string> onError)
    {
        if (!IsAuthenticated())
        {
            Debug.Log("Cannot update field when user is not logged in!");
            onError("User not authenticated");
            return;
        }

        string userId = CurrentUserId();
        db.Child("players").Child(userId).Child(fieldName).SetValueAsync(value)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }

                Debug.Log($"{fieldName} updated successfully!");
                onSuccess();
            });
    }

    /// <summary>
    /// Adds an inventory item for the authenticated user and invokes callbacks based on the operation result.
    /// </summary>
    /// <param name="item">The inventory item to add.</param>
    /// <param name="onSuccess">Callback invoked if the item is added successfully.</param>
    /// <param name="onError">Callback invoked with an error message if the operation fails.</param>
    public void AddInventoryItem(Inventory item, Action onSuccess, Action<string> onError)
    {
        if (!IsAuthenticated())
        {
            Debug.Log("Cannot add item when user is not logged in!");
            onError("User not authenticated");
            return;
        }

        string userId = CurrentUserId();
        string itemKey = db.Child("players").Child(userId).Child("inventoryItems").Push().Key;

        string json = JsonUtility.ToJson(item);

        db.Child("players").Child(userId).Child("inventoryItems").Child(itemKey).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }

                Debug.Log("Inventory item added successfully!");
                onSuccess();
            });
    }

    /// <summary>
    /// Sets the display name for the currently authenticated user in the Firebase database.
    /// </summary>
    /// <param name="displayName">The new display name to set.</param>
    /// <param name="onError">Callback invoked with an error message if the operation fails.</param>
    /// <param name="onSuccess">Callback invoked if the operation succeeds.</param>
    public void SetDisplayName(string displayName, Action<string> onError, Action onSuccess)
    {
        if (!IsAuthenticated())
        {
            Debug.Log("Cannot set display name when user is not logged in!");
            return;
        }

        FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(CurrentUserId()).Child("displayName")
            .SetValueAsync(displayName).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.Log(task.Exception);
                    onError(task.Exception.ToString());
                    return;
                }

                onSuccess();
            });
    }

    /// <summary>
    /// Retrieves the current user's display name from the database and updates the DisplayName property.
    /// </summary>
    public void GetDisplayName()
    {
        if (!IsAuthenticated())
        {
            Debug.Log("Cannot set display name when user is not logged in!");
            return;
        }

        string userId = CurrentUserId();
        db.Child("players").Child(userId).Child("displayName").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                if (task.Exception != null) Debug.Log(task.Exception);
                return;
            }
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                DisplayName = snapshot.Value.ToString();
                Debug.Log("Display name retrieved: " + DisplayName);
            }
        });
    }

    /// <summary>
    /// Saves the complete player data to the database for the authenticated user.
    /// </summary>
    /// <param name="player">The player data to be saved.</param>
    /// <param name="onSuccess">Callback invoked when the save operation completes successfully.</param>
    /// <param name="onError">Callback invoked with an error message if the save operation fails.</param>
    public void SaveCompletePlayerData(Player player, Action onSuccess, Action<string> onError)
    {
        if (!IsAuthenticated())
        {
            onError("User not authenticated");
            return;
        }

        string json = JsonUtility.ToJson(player);
        string userId = CurrentUserId();

        db.Child("players").Child(userId).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }

                Debug.Log("Complete player data saved successfully!");
                onSuccess();
            });
    }

    /// <summary>
    /// Loads the complete player data for the authenticated user from the database and invokes the appropriate callback
    /// based on the result.
    /// </summary>
    /// <param name="onSuccess">Callback invoked with the loaded Player object if retrieval is successful.</param>
    /// <param name="onError">Callback invoked with an error message if retrieval fails or the user is not authenticated.</param>
    public void LoadCompletePlayerData(Action<Player> onSuccess, Action<string> onError)
    {
        if (!IsAuthenticated())
        {
            onError("User not authenticated");
            return;
        }

        string userId = CurrentUserId();
        db.Child("players").Child(userId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        string json = snapshot.GetRawJsonValue();
                        Player player = JsonUtility.FromJson<Player>(json);
                        Debug.Log($"Player loaded: {player.username} with {player.inventoryItems.Count} items");
                        onSuccess(player);
                    }
                    else
                    {
                        onError("Player data not found");
                    }
                }
            });
    }

    /// <summary>
    /// Retrieves all players from the database and returns them as a dictionary keyed by user ID.
    /// </summary>
    /// <param name="onSuccess">Callback invoked with a dictionary of user IDs and Player objects when retrieval succeeds.</param>
    /// <param name="onError">Callback invoked with an error message if retrieval fails.</param>
    public void FetchAllPlayers(Action<Dictionary<string, Player>> onSuccess, Action<string> onError)
    {
        db.Child("players").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.Exception != null) Debug.LogError(task.Exception);
                    onError(task.Exception?.ToString() ?? "Unknown error");
                    return;
                }
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    Dictionary<string, Player> players = new Dictionary<string, Player>();

                    if (!snapshot.Exists || !snapshot.HasChildren)
                    {
                        Debug.Log("No players found in database");
                        onSuccess(players);
                        return;
                    }

                    foreach (DataSnapshot childSnapshot in snapshot.Children)
                    {
                        try
                        {
                            string userId = childSnapshot.Key;
                            string json = childSnapshot.GetRawJsonValue();
                            Player player = JsonUtility.FromJson<Player>(json);

                            if (player != null && !string.IsNullOrEmpty(player.username))
                            {
                                players.Add(userId, player);
                            }
                            else
                            {
                                Debug.LogWarning($"Invalid player data for userId: {userId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error parsing player data: {ex.Message}");
                        }
                    }

                    Debug.Log($"Fetched {players.Count} players successfully!");
                    onSuccess(players);
                }
            });
    }

    /// <summary>
    /// Marks the currently authenticated user as offline in the database.
    /// </summary>
    public void SetUserOffline()
    {
        if (IsAuthenticated())
        {
            db.Child("players").Child(CurrentUserId()).Child("isLoggedIn").SetValueAsync(false)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("User set to offline");
                    }
                });
        }
    }

    private void OnApplicationQuit()
    {
        SetUserOffline();
    }
}