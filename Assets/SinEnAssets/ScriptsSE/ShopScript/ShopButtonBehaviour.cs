/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the ShopButtonBehaviour class for the VR game, which manages the behavior of shop item buttons in the shop UI, allowing players to purchase items from the shop.
*/
using UnityEngine;
using UnityEngine.UI;

public class ShopButtonBehaviour : MonoBehaviour
{
    public ShopItem shopItem;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(ShopItem item)
    {
        shopItem = item;
    }

    /// <summary>
    /// Handles the purchase button click event by attempting to purchase the associated shop item through the
    /// ShopManager.
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        Debug.Log($"Purchase button clicked for: {shopItem.shopItemName}");
        
        if (ShopManager.instance != null && shopItem != null)
        {
            ShopManager.instance.PurchaseItem(shopItem);
        }
        else
        {
            Debug.LogError("ShopManager instance or shopItem is null!");
        }
    }
}