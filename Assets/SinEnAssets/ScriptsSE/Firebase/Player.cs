/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the Player class for the VR game, which represents a player's profile and game data, including username, email, login status, current level, health, coins, and inventory items. 
* The class includes constructors for creating new player instances with default values or specified username and email.
*/
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Player
{
    public string username;
    public string email;
    public bool isLoggedIn;
    public int currentLevel;
    public int currentHealth;
    public int coins;
    public List<Inventory> inventoryItems = new List<Inventory>();

    /// <summary>
    /// Initializes a new instance of the Player class with default inventory, login status, level, health, and coins.
    /// </summary>
    public Player()
    {
        this.inventoryItems = new List<Inventory>();
        this.isLoggedIn = false;
        this.currentLevel = 0;;
        this.currentHealth = 100;
        this.coins = 100;
    }

    /// <summary>
    /// Initializes a new instance of the Player class with the specified username and email, setting default values for
    /// player state.
    /// </summary>
    /// <param name="username">The player's username.</param>
    /// <param name="email">The player's email address.</param>
    public Player(string username, string email)
    {
        this.username = username;
        this.email = email;
        this.isLoggedIn = false;
        this.currentLevel = 0;
        this.currentHealth = 100;
        this.coins = 100;
        this.inventoryItems = new List<Inventory>();
    }
}
