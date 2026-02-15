/*
* Author: Kwek Sin En
* Date: 28/01/2026
* Description: Manages the leaderboard UI for the VR game, fetching player data from Firebase and displaying it in a scrollable list.
*/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References - Assign Prefabs Only")]
    public GameObject leaderboardEntryPrefab;
    private Transform leaderboardContainer;

    [Header("Settings")]
    public int displayCount = 10;

    /// <summary>
    /// Opens the leaderboard by initializing UI references and refreshing the displayed player data.
    /// </summary>
    public void OpenLeaderboard()
    {
        Debug.Log("=== Opening Leaderboard ===");
        FindUIReferences();
        RefreshLeaderboard();
    }

    /// <summary>
    /// Searches the scene for the leaderboard UI elements and assigns the leaderboard container reference.
    /// </summary>
    private void FindUIReferences()
    {
        Debug.Log("Finding leaderboard UI references...");
        
        // Find LeaderboardCanvas in the scene
        GameObject leaderboardCanvas = GameObject.Find("LeaderboardCanvas");
        if (leaderboardCanvas == null)
        {
            Debug.LogError("LeaderboardCanvas not found! Make sure it's active in the scene.");
            return;
        }

        // Method 1: Direct child
        Transform content = leaderboardCanvas.transform.Find("LeaderboardContent");
        if (content != null)
        {
            leaderboardContainer = content;
            Debug.Log("Found leaderboard container: Content");
            return;
        }

        // Method 2: Nested under ScrollView/Viewport
        Transform scrollView = leaderboardCanvas.transform.Find("Leaderboard");
        if (scrollView != null)
        {
            Transform viewport = scrollView.Find("Viewport");
            if (viewport != null)
            {
                content = viewport.Find("LeaderboardContent");
                if (content != null)
                {
                    leaderboardContainer = content;
                    Debug.Log("Found leaderboard container: Leaderboard/Viewport/LeaderboardContent");
                    return;
                }
            }
        }

        // Method 3: Search by name
        Transform[] allChildren = leaderboardCanvas.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "LeaderboardContent")
            {
                leaderboardContainer = child;
                Debug.Log($"Found leaderboard container: {child.name}");
                return;
            }
        }
        Debug.LogError("Could not find leaderboard container! Check your LeaderboardCanvas hierarchy.");
    }

    /// <summary>
    /// Refreshes the leaderboard by fetching all player data from the FirebaseManager.
    /// </summary>
    public void RefreshLeaderboard()
    {
        Debug.Log("Refreshing leaderboard...");
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("FirebaseManager.Instance is null!");
            return;
        }
        FirebaseManager.Instance.FetchAllPlayers(OnPlayersLoadedDictionary, OnError);
    }

    /// <summary>
    /// Handles a loaded dictionary of players by converting it to a list, logging the count, and invoking the player
    /// loaded handler.
    /// </summary>
    /// <param name="playerDict">A dictionary mapping player IDs to Player objects.</param>
    private void OnPlayersLoadedDictionary(Dictionary<string, Player> playerDict)
    {
        List<Player> players = playerDict?.Values.ToList() ?? new List<Player>();
        Debug.Log($"Fetched {players.Count} players for leaderboard");
        OnPlayersLoaded(players);
    }

    /// <summary>
    /// Populates the leaderboard UI with the top players sorted by coin count.
    /// </summary>
    /// <remarks>Ensures required UI references are set and not persistent before updating the leaderboard
    /// entries.</remarks>
    /// <param name="players">The list of players to display on the leaderboard.</param>
    private void OnPlayersLoaded(List<Player> players)
    {
        Debug.Log("=== OnPlayersLoaded called ===");
        
        if (leaderboardContainer == null)
        {
            Debug.LogError("leaderboardContainer is NULL! Call FindUIReferences() first.");
            return;
        }

        if (leaderboardEntryPrefab == null)
        {
            Debug.LogError("leaderboardEntryPrefab is NULL! Assign it in Inspector.");
            return;
        }

        if (IsPersistent(leaderboardContainer.gameObject))
        {
            Debug.LogError("leaderboardContainer is persistent! Cannot instantiate with persistent parent. Make sure LeaderboardCanvas is NOT DontDestroyOnLoad.");
            return;
        }

        Debug.Log($"Clearing existing entries from: {leaderboardContainer.name}");
        
        // Clear existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Sort by coins descending
        List<Player> sorted = players
            .OrderByDescending(p => p.coins)
            .Take(displayCount)
            .ToList();

        Debug.Log($"Displaying top {sorted.Count} players");

        // Spawn and populate each entry
        for (int i = 0; i < sorted.Count; i++)
        {
            GameObject entry = Instantiate(leaderboardEntryPrefab);
            entry.transform.SetParent(leaderboardContainer, false);
            LeaderboardEntry leaderboardEntry = entry.GetComponent<LeaderboardEntry>();
            if (leaderboardEntry != null)
            {
                leaderboardEntry.SetEntry(i + 1, sorted[i].username, sorted[i].coins);
                Debug.Log($"#{i + 1}: {sorted[i].username} - {sorted[i].coins} coins");
            }
            else
            {
                Debug.LogError("LeaderboardEntry component not found on prefab!");
            }
        }
        Debug.Log("Leaderboard displayed successfully");
    }

    /// <summary>
    /// Logs an error message when leaderboard fetching fails.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    private void OnError(string error)
    {
        Debug.LogError($"Leaderboard fetch failed: {error}");
    }

    /// <summary>
    /// Determines whether the specified GameObject is persistent across scenes or not saved.
    /// </summary>
    /// <param name="obj">The GameObject to check for persistence.</param>
    /// <returns>True if the GameObject is persistent or marked with DontSave; otherwise, false.</returns>
    private bool IsPersistent(GameObject obj)
    {
        return obj.scene.name == null || obj.hideFlags == HideFlags.DontSave;
    }
}