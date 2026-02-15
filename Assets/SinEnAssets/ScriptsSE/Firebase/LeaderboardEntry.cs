/*
* Author: Kwek Sin En
* Date: 28/01/2026
* Description: Defines the LeaderboardEntry class for the VR game, which represents a single entry in the leaderboard UI.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntry : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text usernameText;
    public TMP_Text coinText;

    /// <summary>
    /// Updates the UI entry with the specified rank, username, and coin count.
    /// </summary>
    /// <param name="rank">The rank to display.</param>
    /// <param name="username">The username to display.</param>
    /// <param name="coins">The number of coins to display.</param>
    public void SetEntry(int rank, string username, int coins)
    {
        rankText.text = rank.ToString();
        usernameText.text = username;
        coinText.text = coins.ToString("N0");
    }
}
