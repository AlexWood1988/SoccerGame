using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitManager : MonoBehaviour
{
    void Start()
    {
        //PlayerPrefs.DeleteAll();
        StartCoroutine(LoadNextLevel());
    }

    public IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Menu-c#");
    }
}
