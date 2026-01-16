using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("01 - Player", LoadSceneMode.Additive);
        SceneManager.LoadScene("02 - Space", LoadSceneMode.Additive);
        SceneManager.LoadScene("02.1 - Lamps", LoadSceneMode.Additive);
    }
}
