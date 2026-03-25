using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        if (!SceneExists(sceneName))
        {
            Debug.LogError("La escena no existe o no esta en la build: " + sceneName);
            return;
        }

        ScreenFader.Instance.StartCoroutine(LoadCoroutineFader(sceneName));
    }

    public static void Reload()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }

    private static IEnumerator LoadCoroutineFader(string sceneName)
    {
        yield return ScreenFader.FadeOut(0.3f);

        SceneManager.LoadScene(sceneName);

        yield return ScreenFader.FadeIn(0.3f);
    }

    private static bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (name == sceneName)
                return true;
        }
        return false;
    }
}
