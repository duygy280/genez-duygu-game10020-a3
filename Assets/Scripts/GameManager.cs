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

    bool gameEnded = false;

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == character && !gameEnded)
        {
            Win();
        }
    }

    public void GameOver()
    {
        if (gameEnded) return;

        gameEnded = true;

        gameOverText.SetActive(true);
        restartButton.SetActive(true);
    }

    void Win()
    {
        gameEnded = true;

        winText.SetActive(true);
        restartButton.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
 
    }

}