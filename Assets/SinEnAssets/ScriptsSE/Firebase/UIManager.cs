/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the UIManager class for the VR game, which manages the various user interface canvases in the game, 
* allowing for easy switching between different UI screens such as login, signup, inventory, leaderboard, shop, and level selection. 
* The class also implements a singleton pattern to ensure only one instance of the UIManager exists throughout the game.
*/
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
    private GameObject loginCanvas;
    [SerializeField]
    private GameObject signupCanvas;
    [SerializeField]
    public GameObject inventoryCanvas;
    [SerializeField]
    private GameObject leaderboardCanvas;
    [SerializeField]
    private GameObject shopCanvas;
    [SerializeField]
    public GameObject levelCanvas;

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

    void Start()
    {
        ShowLogin();
    }

    /// <summary>
    /// Activates or deactivates the specified canvas GameObject.
    /// </summary>
    /// <param name="canvas">The canvas GameObject to modify.</param>
    /// <param name="active">True to activate the canvas; false to deactivate.</param>
    private void SetCanvasActive(GameObject canvas, bool active)
    {
        if (canvas != null)
        {
            canvas.SetActive(active);
        }
    }
    
    /// <summary>
    /// Displays the login canvas and hides the signup, inventory, leaderboard, shop, and level canvases.
    /// </summary>
    public void ShowLogin()
    {
        SetCanvasActive(loginCanvas, true);
        SetCanvasActive(signupCanvas, false);
        SetCanvasActive(inventoryCanvas, false);
        SetCanvasActive(leaderboardCanvas, false);
        SetCanvasActive(shopCanvas, false);
        SetCanvasActive(levelCanvas, false);
    }
    
    /// <summary>
    /// Displays the signup canvas and hides all other canvases.
    /// </summary>
    public void ShowSignup()
    {
        SetCanvasActive(loginCanvas, false);
        SetCanvasActive(signupCanvas, true);
        SetCanvasActive(inventoryCanvas, false);
        SetCanvasActive(leaderboardCanvas, false);
        SetCanvasActive(shopCanvas, false);
        SetCanvasActive(levelCanvas, false);
    }
    
    /// <summary>
    /// Displays the inventory UI by activating the inventory canvas and deactivating other canvases.
    /// </summary>
    public void ShowInventory()
    {
        SetCanvasActive(loginCanvas, false);
        SetCanvasActive(signupCanvas, false);
        SetCanvasActive(inventoryCanvas, true);
        SetCanvasActive(leaderboardCanvas, false);
        SetCanvasActive(shopCanvas, false);
        SetCanvasActive(levelCanvas, false);
        InvenManager.instance.OpenInventoryUI();
    }
    
    /// <summary>
    /// Displays the leaderboard canvas and hides all other canvases.
    /// </summary>
    public void ShowLeaderboard()
    {
        SetCanvasActive(loginCanvas, false);
        SetCanvasActive(signupCanvas, false);
        SetCanvasActive(inventoryCanvas, false);
        SetCanvasActive(leaderboardCanvas, true);
        SetCanvasActive(shopCanvas, false);
        SetCanvasActive(levelCanvas, false);
    }
    
    /// <summary>
    /// Displays the level canvas and hides all other UI canvases.
    /// </summary>
    public void ShowLevel()
    {
        SetCanvasActive(loginCanvas, false);
        SetCanvasActive(signupCanvas, false);
        SetCanvasActive(inventoryCanvas, false);
        SetCanvasActive(leaderboardCanvas, false);
        SetCanvasActive(shopCanvas, false);
        SetCanvasActive(levelCanvas, true);
    }

    /// <summary>
    /// Displays the shop canvas and hides all other UI canvases.
    /// </summary>
    public void ShowShop()
    {
        SetCanvasActive(loginCanvas, false);
        SetCanvasActive(signupCanvas, false);
        SetCanvasActive(inventoryCanvas, false);
        SetCanvasActive(leaderboardCanvas, false);
        SetCanvasActive(shopCanvas, true);
        SetCanvasActive(levelCanvas, false);
    }

    /// <summary>
    /// Closes the inventory by deactivating the inventory canvas.
    /// </summary>
    public void CloseInventory()
    {
        SetCanvasActive(inventoryCanvas, false);
    }

    /// <summary>
    /// Closes the leaderboard by deactivating its canvas.
    /// </summary>
    public void CloseLeaderboard()
    {
        SetCanvasActive(leaderboardCanvas, false);
    }

    /// <summary>
    /// Closes the shop by deactivating the shop canvas.
    /// </summary>
    public void CloseShop()
    {
        SetCanvasActive(shopCanvas, false);
    }

    /// <summary>
    /// Closes the current level by deactivating the level canvas.
    /// </summary>
    public void CloseLevel()
    {         
        SetCanvasActive(levelCanvas, false);
    }
}
