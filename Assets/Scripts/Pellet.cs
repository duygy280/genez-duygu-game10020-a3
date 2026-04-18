using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pellet : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pellet touched by: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Pellet collected!");

            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.CollectPellet();
            }

            Destroy(gameObject);
        }
    }
}