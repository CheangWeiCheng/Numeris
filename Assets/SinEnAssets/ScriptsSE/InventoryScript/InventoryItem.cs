/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the InventoryItem class for the VR game, which represents an item that can be stored in the player's inventory.
*/
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "Scriptable Objects/InventoryItem")]
public class InventoryItem : ScriptableObject
{
    public int invenId;
    public string invenItemName;
    public int invenQuantity;
    public Sprite invenIcon;
    public ItemTier itemTier;
    public int dropChance;
    public GameObject lootPrefab3D;
    public PowerUpType powerUpType;
}

/// <summary>
/// Specifies the tier or rarity level of an item.
/// </summary>
public enum ItemTier
{
    Common,
    Rare,
    Epic
}

/// <summary>
/// Specifies the available types of power-ups.
/// </summary>
public enum PowerUpType
{
    None,
    HealthPotion,
    FiftyFifty,
    SwitchQuestion
}