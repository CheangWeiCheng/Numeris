/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the EnemyBehaviour class for the VR game, which manages the behavior of enemy characters in the game, including taking damage, dying with a dissolve effect, dropping loot and coins upon death, and interacting with the Portal and BossKeyDrop systems. 
* The class also handles disabling correct orbs when an enemy dies.
*/
using System.Collections;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private LootBag lootBag;
    public DissolveController dissolveController;
    public int maxHealth = 1;
    public int currentHealth;
    public BossKeyDrop bossKeyDrop;
    
    private void Start()
    {
        currentHealth = maxHealth;
        lootBag = GetComponent<LootBag>();
    }
    
    /// <summary>
    /// Reduces current health by the specified damage amount and triggers death if health reaches zero or below.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles the enemy's death by logging the event, notifying the portal, awarding coins, dropping loot and keys if
    /// applicable, triggering dissolve effects or destroying the object, and deactivating all objects tagged as
    /// 'CorrectOrb'.
    /// </summary>
    void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        Portal.Instance?.OnEnemyKilled();
        GetCoinsOnDeath();
        if (dissolveController != null)
        {
            if (lootBag != null)
            {
                lootBag.DropLoot();
            }
            StartCoroutine(DieWithDissolve());
        }
        else
        {
            Destroy(gameObject);
        }
        if (bossKeyDrop != null)
        {
            bossKeyDrop.DropKey();
        }
        GameObject[] correctOrbs = GameObject.FindGameObjectsWithTag("CorrectOrb");
        foreach (GameObject correctOrb in correctOrbs)
        {
            if (correctOrb != null)
            {
                correctOrb.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Handles the enemy's death by disabling its collider, playing a dissolve effect, and destroying the game object.
    /// </summary>
    /// <returns>An enumerator for coroutine execution.</returns>
    IEnumerator DieWithDissolve()
    {
        // Disable collider so enemy can't be damaged while dissolving
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        yield return StartCoroutine(dissolveController.DissolveEffect());
        Destroy(gameObject);
    }

    /// <summary>
    /// Awards the player a random number of coins between 5 and 15 upon death if the PlayerManager instance exists.
    /// </summary>
    void GetCoinsOnDeath()
    {
        if (PlayerManager.Instance != null)
        {
            int coinsToGive = Random.Range(5, 16); // Random coins between 5 and 15
            PlayerManager.Instance.AddCoins(coinsToGive);
            Debug.Log($"Player received {coinsToGive} coins from {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("PlayerManager instance not found. Cannot add coins.");
        }
    }
}