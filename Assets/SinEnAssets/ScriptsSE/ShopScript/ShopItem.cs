/*
* Author: Kwek Sin En
* Date: 22/02/2026
* Description: Defines the ShopItem class for the VR game, which represents an item available for purchase in the shop, 
* including its properties such as name, price, icon, type, tier, and the corresponding inventory item that will be added to the player's inventory upon purchase.
*/
using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Scriptable Objects/ShopItem")]
public class ShopItem : ScriptableObject
{
    public int shopItemId;
    public string shopItemName;
    public int price;
    public Sprite shopItemIcon;
    public ShopItemType itemType;
    public ItemTier itemTier;
    
    /// <summary>
    /// Specifies the types of items available in the shop.
    /// </summary>
    public enum ShopItemType
    {
        Consumable,
        PowerUp,
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
    /// Represents the associated inventory item.
    /// </summary>
    public InventoryItem correspondingInventoryItem;
}
