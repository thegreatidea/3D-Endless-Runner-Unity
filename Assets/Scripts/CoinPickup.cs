using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class CoinPickup : MonoBehaviour
{
    public ParticleSystem collectEffect;
    public AudioClip collectSound; // Sound to play when coin is collected


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show particle
            if (collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            // Play sound
            SoundFXManager.Instance.PlaySoundEffect(collectSound, transform, 0.5f);
            // Add to score
            GameManager.Instance.AddCoin(1); // Make sure GameManager exists
            StartCoroutine(ResetCoins());
          
        }
    }

    IEnumerator ResetCoins()
    {
        gameObject.GetComponentInChildren<Renderer>().enabled = false;
        yield return new WaitForSeconds(2.5f);
        gameObject.GetComponentInChildren<Renderer>().enabled = true;
    }
    
}
