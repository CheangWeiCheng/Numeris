/*
* Author: Kwek Sin En
* Date: 22/01/2026
* Description: Defines the Staff class for the VR game, which manages the behavior of the staff weapon, allowing players to shoot projectiles towards a target point in the game world. 
* The class also checks if the correct orb is attached before allowing the player to shoot.
*/
using UnityEngine;

public class Staff : MonoBehaviour
{
    public Camera cam;
    public float speed = 10;
    public GameObject projectile; 
    public Transform activationPoint;
    public Vector3 destination;
    
    public AnswerDetection answerDetection;
    
    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }
    
    /// <summary>
    /// Fires a projectile towards the point the camera is aiming at if the correct orb is attached.
    /// </summary>
    public void ShootProjectile()
    {
        if (answerDetection == null || !answerDetection.IsCorrectOrbAttached)
        {
            Debug.Log("Cannot shoot - no correct orb attached!");
            return;
        }
        
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            destination = hit.point;
        }
        else
        {
            destination = ray.GetPoint(1000);
        }
        InstantiateProjectile();
        AudioManager.Instance.PlayLaserBeam();
    }
    
    /// <summary>
    /// Instantiates a projectile at the activation point and sets its velocity toward the destination at the specified
    /// speed.
    /// </summary>
    void InstantiateProjectile()
    {
        GameObject proj = Instantiate(projectile, activationPoint.position, Quaternion.identity);
        proj.GetComponent<Rigidbody>().linearVelocity = (destination - activationPoint.position).normalized * speed;
    }
}