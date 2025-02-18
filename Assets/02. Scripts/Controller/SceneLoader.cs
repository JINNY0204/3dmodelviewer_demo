using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(int index)
    {
        DOTween.KillAll();
        DOTween.ClearCachedTweens();
        DOTween.Clear();
        SceneManager.LoadScene(index);
    }
}