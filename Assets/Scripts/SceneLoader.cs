using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("XX - Testing 1", LoadSceneMode.Additive);
        SceneManager.LoadScene("XX - Testing 2", LoadSceneMode.Additive);
    }
}
