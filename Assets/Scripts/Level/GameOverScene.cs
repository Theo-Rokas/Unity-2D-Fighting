using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScene : MonoBehaviour
{
    public float timer = 3;

    void Start()
    {
        StartCoroutine("LoadScene");
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(timer);
        MySceneManager.GetInstance().RequestLevelLoad(MySceneManager.SceneType.main, "intro");
    }
}
