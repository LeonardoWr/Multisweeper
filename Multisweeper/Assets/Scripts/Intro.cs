using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Intro : MonoBehaviour
{
    public float delayTime = 3f;

    void Start()
    {
        StartCoroutine(DelayedAction());
    }

    IEnumerator DelayedAction()
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(1);
    }
}
