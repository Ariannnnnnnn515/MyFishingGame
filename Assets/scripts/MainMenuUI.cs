using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Gameplay";

    public void StartGame()
    {
        print("Начинаем игру");
        SceneManager.LoadScene(sceneToLoad);
    }

    public void ExitGame()
    {
        print("Выходим из игры");
        Application.Quit();
    }
}