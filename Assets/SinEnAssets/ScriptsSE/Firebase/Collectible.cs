/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the Collectible class, which represents a collectible item in the VR game.
*/
using UnityEngine;
using System;

[Serializable]
public class Collectible
{
    public string collectibleID;
    public string collectibleName;
    public string tier;
    public int quantity;

    /// <summary>
    /// Initializes a new instance of the Collectible class.
    /// </summary>
    public Collectible() { }

    /// <summary>
    /// Initializes a new instance of the Collectible class with the specified ID, name, tier, and quantity.
    /// </summary>
    /// <param name="collectibleID">The unique identifier for the collectible.</param>
    /// <param name="collectibleName">The display name of the collectible.</param>
    /// <param name="tier">The tier or rarity of the collectible.</param>
    /// <param name="quantity">The quantity of the collectible.</param>
    public Collectible(string collectibleID, string collectibleName, string tier, int quantity)
    {
        this.collectibleID = collectibleID;
        this.collectibleName = collectibleName;
        this.tier = tier;
        this.quantity = quantity;
    }
}
