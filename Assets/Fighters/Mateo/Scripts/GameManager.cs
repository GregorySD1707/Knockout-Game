using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mono.Data.Sqlite;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManagerInstance;

    [SerializeField] public List<FightersData> fightersData;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private AudioClip knockoutVoice;
    [SerializeField] private GameObject puaseUI;
    [SerializeField] private KeyCode pauseKey = KeyCode.P;

    [SerializeField] private AudioClip fighterSelectionAudio;

    private void Awake()
    {
        if (GameManager.gameManagerInstance == null)
        {
            GameManager.gameManagerInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "FighterSelectionMenu")
        {
            PlayerPrefs.SetString("User1", "");
            PlayerPrefs.Save();
            PlayerPrefs.SetString("User2", "");
            PlayerPrefs.Save();
            SoundsController.Instance.RunSound(fighterSelectionAudio);

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey) && puaseUI != null && !gameOverUI.activeSelf)
        {
            puaseUI.SetActive(!puaseUI.activeSelf);
            Time.timeScale = puaseUI.activeSelf ? 0 : 1;

            if (puaseUI.activeSelf)
            {
                SoundsController.Instance.pauseSound();  // Pausar todos los sonidos
            }
            else
            {
                SoundsController.Instance.reactiveSound();  // Reanudar todos los sonidos
            }
        }
    }

    public void enableGameOverPanel(string looserUserTag)
    {
        string winnerUserTag = looserUserTag == "User1" ? "User2" : "User1";
        GameObject winner = GameObject.FindGameObjectWithTag(winnerUserTag);
        //gameOverUI.setSpriteRenderer(winner.GetComponent<SpriteRenderer>());
        Image winnerImage = winner.GetComponent<Image>();
        Transform childTransform = gameOverUI.transform.transform.Find("WinnerImage");
        childTransform.GetComponent<Image>().sprite = winnerImage.sprite;

        gameOverUI.SetActive(true);
        Time.timeScale = 0;
        //SoundsController.Instance.PauseAllSounds();  // Pausar todos los sonidos
        SoundsController.Instance.RunSound(knockoutVoice);

        TextMeshProUGUI textMeshProUGUI = gameOverUI.transform.Find("KnockoutTMP").GetComponent<TextMeshProUGUI>();

        if (winnerUserTag == "User1" && !string.IsNullOrEmpty(PlayerPrefs.GetString("User1")))
        {
            string nameWinner = PlayerPrefs.GetString("User1");
            textMeshProUGUI.text = nameWinner + " is the winner!";
            Debug.Log("El ganador es: " + nameWinner);
            updateWinnerScore(nameWinner);
        }
        else if (winnerUserTag == "User2" && !string.IsNullOrEmpty(PlayerPrefs.GetString("User2")))
        {
            string nameWinner = PlayerPrefs.GetString("User2");
            textMeshProUGUI.text = nameWinner + " is the winner!";
            Debug.Log("El ganador es: " + nameWinner);
            updateWinnerScore(nameWinner);
        }
        else if (winnerUserTag == "User1")
        {
            Debug.Log("Valor de User1: " + PlayerPrefs.GetString("User1"));
            textMeshProUGUI.text = "Player 1 is the winner!";
        }
        else
        {
            Debug.Log("Valor de User2: " + PlayerPrefs.GetString("User2"));
            textMeshProUGUI.text = "Player 2 is the winner!";
        }
    }

    public void resume()
    {
        puaseUI.SetActive(false);
        Time.timeScale = 1;
        SoundsController.Instance.reactiveSound();  // Reanudar todos los sonidos
        //SoundsController.Instance.ResumeAllSounds();  // Reanudar todos los sonidos
        //SoundsController.Instance.RunSound(pauseSound);
    }

    public void restartGame()
    {
        Debug.Log("Restarting game...");
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;

    }

    public void goToMainMenu()
    {
        // Al volver al menú principal, asegúrate de que la música se reanude
        if (MenuMusicManager.Instance != null)
        {
            MenuMusicManager.Instance.ResumeMusic();
        }
        
        PlayerPrefs.SetString("User1", "");
        PlayerPrefs.Save();
        PlayerPrefs.SetString("User2", "");
        PlayerPrefs.Save();


        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
    }

    public void goToFighterSelectionMenu()
    {
        // Al volver al menú principal, asegúrate de que la música se reanude
        if (MenuMusicManager.Instance != null)
        {
            MenuMusicManager.Instance.ResumeMusic();
        }

        PlayerPrefs.SetString("User1", "");
        PlayerPrefs.Save();
        PlayerPrefs.SetString("User2", "");
        PlayerPrefs.Save();

        SceneManager.LoadScene("FighterSelectionMenu");
        Time.timeScale = 1; 
    }

    private void updateWinnerScore(string username)
    {
        string dbName = "URI=file:LeaderboardDB.db"; // Ruta de la base de datos

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                // Sumar 5 al score del usuario en una sola consulta
                command.CommandText = "UPDATE User SET score = score + 5 WHERE username = @username";
                command.Parameters.AddWithValue("@username", username);
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                    Debug.Log("Puntaje actualizado para: " + username);
                else
                    Debug.LogError("Usuario no encontrado en la base de datos: " + username);
            }

            connection.Close();
        }
    }

}

