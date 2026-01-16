using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlackFadeTransition : MonoBehaviour
{
    public Image blackImage;
    public float fadeDuration = 1f;

    public IEnumerator FadeIn()
    {
        float t = 0;
        Color c = blackImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            blackImage.color = c;
            yield return null;
        }
        c.a = 1;
        blackImage.color = c;
    }

    public IEnumerator FadeOut()
    {
        float t = 0;
        Color c = blackImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            blackImage.color = c;
            yield return null;
        }
        c.a = 0;
        blackImage.color = c;
    }
}
