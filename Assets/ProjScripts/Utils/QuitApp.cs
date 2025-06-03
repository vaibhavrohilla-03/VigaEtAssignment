using UnityEngine;

public class QuitApp : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("QuitApp");
        Application.Quit();
    }

}
