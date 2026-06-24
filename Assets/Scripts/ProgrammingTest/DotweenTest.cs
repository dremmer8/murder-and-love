using System;
using UnityEngine;
using DG.Tweening;

public class DotweenTest : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    private Vector3 originalCameraPosition;

    private void Start()
    {
        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.localPosition;
        }
    }

    public void TriggerCameraShake(float duration, float strength, int vibrato, float randomness, bool fadeOut = true)
    {
        if (playerCamera != null)
        {
            playerCamera.transform.DOComplete();
            
            playerCamera.transform.localPosition = originalCameraPosition;
            
            playerCamera.transform.DOShakePosition(
                duration, 
                strength, 
                vibrato, 
                randomness, 
                false, 
                fadeOut
            );
        }
    }
}