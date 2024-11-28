using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        //Debug line to test quit function in editor
        //UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Quit pressed");
        Application.Quit();
    }
}
