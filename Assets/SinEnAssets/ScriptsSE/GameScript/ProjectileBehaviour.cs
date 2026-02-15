/*
* Author: Kwek Sin En
* Date: 21/01/2026
* Description: Defines the ProjectileBehaviour class for the VR game, which manages the behavior of projectiles fired by the staff weapon.
*/
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public GameObject impactVFX;
    public int damage = 1;
    private bool hasCollided;

    /// <summary>
    /// Handles collision events, applying damage to enemies, spawning impact effects, and destroying the object as
    /// appropriate.
    /// </summary>
    /// <param name="collision">Collision data associated with the contact event.</param>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && !hasCollided)
        {
            hasCollided = true;
            GameObject impact = Instantiate(impactVFX, collision.contacts[0].point, Quaternion.identity);
            Destroy(impact, 2f);
            EnemyBehaviour enemy = collision.gameObject.GetComponent<EnemyBehaviour>();
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }
}
