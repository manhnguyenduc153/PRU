using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject instructionPanel;
    public CanvasGroup instructionGroup;

    public void StartGame()
    {
        SceneManager.LoadScene("Scene1");
    }

    // public void ShowInstructions()
    // {
    //     instructionPanel.SetActive(true);
    // }

    // public void HideInstructions()
    // {
    //     instructionPanel.SetActive(false);
    // }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game exited."); // chỉ thấy khi chạy trong editor
    }

    public void ShowInstructions()
    {
        instructionGroup.alpha = 1;
        instructionGroup.interactable = true;
        instructionGroup.blocksRaycasts = true;
    }

    public void HideInstructions()
    {
        instructionGroup.alpha = 0;
        instructionGroup.interactable = false;
        instructionGroup.blocksRaycasts = false;
    }
}
