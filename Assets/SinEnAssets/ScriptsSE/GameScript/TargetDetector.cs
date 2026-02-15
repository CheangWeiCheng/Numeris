/*
* Author: Kwek Sin En
* Date: 21/01/2026
* Description: Detects potential targets within a specified radius and field of view.
*/

using UnityEngine;
using System.Collections.Generic;

public class TargetDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private float fieldOfViewAngle = 90f; // Angle in degrees

    /// <summary>
    /// Finds and returns a list of game objects within a specified detection radius and field of view angle from the
    /// player's camera.
    /// </summary>
    /// <returns>A list of potential target game objects detected within range and field of view.</returns>
    public List<GameObject> FindPotentialTargets()
    {
        List<GameObject> potentialTargets = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);

        Debug.Log($"OverlapSphere found: {hitColliders.Length} colliders");
        
        
        foreach (var hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - playerCameraTransform.position).normalized;
            float angle = Vector3.Angle(playerCameraTransform.forward, directionToTarget);
            
            // If enemy is within FOV cone (e.g., 90 degrees), add to list
            if (angle < fieldOfViewAngle / 2)
            {
                potentialTargets.Add(hitCollider.gameObject);
            }
        }
        
        return potentialTargets;
    }

    /// <summary>
    /// Draws a yellow wireframe sphere in the Scene view to visualize the detection radius when the object is selected.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}