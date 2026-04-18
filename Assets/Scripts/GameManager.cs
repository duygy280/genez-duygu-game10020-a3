using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject character;

    public GameObject gameOverText;
    public GameObject winText;
    public GameObject restartButton;

    //added pellet system
    public int totalPellets;
    int collectedPellets = 0;

    bool gameEnded = false;

    void Start()
    {
        //hide UI at start
        gameOverText.SetActive(false);
        winText.SetActive(false);
        restartButton.SetActive(false);

        //count all pellets
        Pellet[] pellets = FindObjectsOfType<Pellet>();
        totalPellets = pellets.Length;
        Debug.Log("Total pellets found: " + totalPellets);
    }

    //called when player reaches exit 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == character && !gameEnded)
        {
            Win();
        }
    }

    //called by ai when player is caught
    public void GameOver()
    {
        if (gameEnded) return;

        gameEnded = true;

        gameOverText.SetActive(true);
        restartButton.SetActive(true);
    }

    //called when all pellets are collected
    void Win()
    {
        if (gameEnded) return;

        gameEnded = true;

        winText.SetActive(true);
        restartButton.SetActive(true);
    }

    public void CollectPellet()
    {
        collectedPellets++;
        Debug.Log("Pellets: " + collectedPellets + " / " + totalPellets);

        if (collectedPellets >= totalPellets)
        {
            Win();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}