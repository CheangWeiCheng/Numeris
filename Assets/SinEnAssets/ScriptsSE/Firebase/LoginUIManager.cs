/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the LoginUIManager class for the VR game, which manages the user interface for logging in with Firebase Authentication.
*/
using TMPro;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;

public class LoginUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private TMP_Text title;
    [SerializeField]
    private TMP_InputField emailField;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private TMP_Text errorText;

    /// <summary>
    /// Validates user input and attempts to log in with the provided email and password using Firebase authentication.
    /// On successful login, updates the user's login status, loads player data and inventory, and transitions to the
    /// main game UI.
    /// </summary>
    public void Login()
    {
        // Obtain text from input fields
        var email = emailField.text;
        var password = passwordField.text;

        // Input validation
        if (email.Length == 0)
        {
            ShowError("E-mail address cannot be empty");
            return;
        }
        if (password.Length == 0)
        {
            ShowError("Password cannot be empty");
            return;
        }
        if (!email.Contains("@") || !email.Contains("."))
        {
            ShowError("Empty or invalid e-mail address");
            return;
        }
        if (password.Length < 6)
        {
            ShowError("Password must be at least 6 characters long");
            return;
        }
        else
        {
            ShowError(""); // Clear error
        }

        // Authenticate with Firebase
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    ShowError($"Login failed: {task.Exception?.GetBaseException().Message}");
                    return;
                }
                if (task.IsCanceled)
                {
                    ShowError("Login was cancelled.");
                    return;
                }
                if (task.IsCompleted)
                {
                    ShowError("");

                    // Set user as logged in
                    FirebaseManager.Instance.UpdatePlayerField("isLoggedIn", true,
                        onSuccess: () =>
                        {
                            Debug.Log("User marked as logged in");
                        },
                        onError: (error) =>
                        {
                            Debug.LogError("Failed to update login status: " + error);
                        }
                    );
                    FirebaseManager.Instance.LoadCompletePlayerData(
                        onSuccess: (player) =>
                        {
                            AudioManager.Instance.PlayLoginSuccessful();
                            PlayerManager.Instance.SetPlayerData(player);
                            InvenManager.instance.LoadInventoryFromFirebase();
                            UIManager.Instance.ShowGame();
                        },
                        onError: (error) =>
                        {
                            Debug.LogError("Failed to load player data: " + error);
                            ShowError("Failed to load player data");
                        }
                    );
                }
            });
    }

    /// <summary>
    /// Displays the specified error message in the error text UI element.
    /// </summary>
    /// <param name="error">The error message to display.</param>
    private void ShowError(string error)
    {
        errorText.text = error;
    }
}
