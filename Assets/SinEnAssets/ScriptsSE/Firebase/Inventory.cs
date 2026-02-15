/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the Inventory class for the VR game, 
* which represents an item in the player's inventory.
*/
using UnityEngine;
using System;

[Serializable]
public class Inventory
{
    public string itemID;
    public Collectible collectibleDetails;
    public Inventory() { }

    public Inventory(string itemID)
    {
        this.itemID = itemID;
        this.collectibleDetails = new Collectible();
    }
}
