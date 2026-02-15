/*
* Author: Kwek Sin En
* Date: 25/01/2026
* Description: Defines the DissolveController class for the VR game, which manages the dissolve effect on a skinned mesh renderer, allowing for a gradual dissolve effect by incrementally increasing the "_Dissolve_Amount" shader property over time. 
* The class includes a coroutine to handle the dissolve effect and allows for customization of the dissolve rate.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DissolveController : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    private Material[] skinnedMaterials;
    public float dissolveRate = 0.0125f;
    
    void Start()
    {
        if (skinnedMeshRenderer != null)
        {
            skinnedMaterials = skinnedMeshRenderer.materials;
        }
    }

    /// <summary>
    /// Gradually increases the dissolve amount on all skinned materials to create a dissolve visual effect.
    /// </summary>
    /// <returns>An enumerator for coroutine execution.</returns>
    public IEnumerator DissolveEffect()
    {
        if (skinnedMaterials.Length >0)
        {
            float counter = 0;
            while (skinnedMaterials[0].GetFloat("_Dissolve_Amount") < 1)
            {
                counter += dissolveRate;
                for (int i=0; i<skinnedMaterials.Length; i++)
                {
                    skinnedMaterials[i].SetFloat("_Dissolve_Amount", counter);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
