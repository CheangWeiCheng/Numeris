/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the PlayerManager class for the VR game, which manages the player's data and interactions with the shop system.
*/
using UnityEngine;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    private Player currentPlayer;

    private void Awake()
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

    /// <summary>
    /// Sets the current player and notifies the ShopManager if available.
    /// </summary>
    /// <param name="player">The player data to set as the current player.</param>
    public void SetPlayerData(Player player)
    {
        currentPlayer = player;
        if (ShopManager.instance != null)
        {
            ShopManager.instance.OnPlayerDataLoaded();
        }
    }

    /// <summary>
    /// Retrieves the current player data.
    /// </summary>
    /// <returns>The current Player instance.</returns>
    public Player GetPlayerData()
    {
        return currentPlayer;
    }

    /// <summary>
    /// Retrieves the current number of coins for the player.
    /// </summary>
    /// <returns>The player's coin count if available; otherwise, returns 100.</returns>
    public int GetCoins()
    {
        //Return current coins from firebase
        if (currentPlayer != null)
        {
            return currentPlayer.coins;
        }
        return 100;
    }

    /// <summary>
    /// Sets the player's coin count to the specified amount and updates it in Firebase.
    /// </summary>
    /// <param name="amount"></param>
    public void SetCoins(int amount)
    {
        if (currentPlayer != null)
        {
            currentPlayer.coins = amount;
        }
    }

    /// <summary>
    /// Adds the specified number of coins to the current player, updates the coin display in the shop, and synchronizes
    /// the new coin total with Firebase.
    /// </summary>
    /// <param name="amount">The number of coins to add to the current player's balance.</param>
    public void AddCoins(int amount)
    {
        if (currentPlayer != null)
        {
            currentPlayer.coins += amount;
            if (ShopManager.instance != null)
            {
                ShopManager.instance.UpdatePlayerCoinsDisplay(currentPlayer.coins);
            }
            // Update in Firebase
            FirebaseManager.Instance.UpdatePlayerField("coins", currentPlayer.coins,
                onSuccess: () => Debug.Log($"Added {amount} coins. Total: {currentPlayer.coins}"),
                onError: (error) => Debug.LogError("Failed to update coins: " + error)
            );
        }
    }

    /// <summary>
    /// Attempts to deduct the specified number of coins from the current player and updates the value in Firebase.
    /// </summary>
    /// <param name="amount">The number of coins to spend.</param>
    /// <returns>True if the coins were successfully spent and updated; otherwise, false.</returns>
    public bool SpendCoins(int amount)
    {
        if (currentPlayer != null && currentPlayer.coins >= amount)
        {
            currentPlayer.coins -= amount;
            
            // Update in Firebase
            FirebaseManager.Instance.UpdatePlayerField("coins", currentPlayer.coins,
                onSuccess: () => Debug.Log($"Spent {amount} coins. Remaining: {currentPlayer.coins}"),
                onError: (error) => Debug.LogError("Failed to update coins: " + error)
            );
            
            return true;
        }
        return false;
    }

    /// <summary>
    /// Retrieves the current health of the player.
    /// </summary>
    /// <returns>The player's current health if available; otherwise, 100.</returns>
    public int GetHealth()
    {
        if (currentPlayer != null)
        {
            return currentPlayer.currentHealth;
        }
        return 100;
    }

    /// <summary>
    /// Sets the current player's health and updates the value in Firebase.
    /// </summary>
    /// <param name="health">The new health value to assign to the current player.</param>
    public void SetHealth(int health)
    {
        if (currentPlayer != null)
        {
            currentPlayer.currentHealth = health;
            
            // Update in Firebase
            FirebaseManager.Instance.UpdatePlayerField("currentHealth", currentPlayer.currentHealth,
                onSuccess: () => Debug.Log($"Health updated to: {currentPlayer.currentHealth}"),
                onError: (error) => Debug.LogError("Failed to update health: " + error)
            );
        }
    }
}