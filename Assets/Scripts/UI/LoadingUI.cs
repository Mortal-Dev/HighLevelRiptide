using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using HLRiptide;

public class LoadingUI : UI
{
    private Slider loadingBarSlider;

    public override void Initialize()
    {
        loadingBarSlider = GetComponentInChildren<Slider>();

        NetworkManager.Singleton.OnLocalClientBeginSceneLoad += OnClientLoadScene;
    }

    private void OnClientLoadScene(AsyncOperation asynCoperation)
    {
        StartCoroutine(LoadingBarEnumerator(asynCoperation));
    }

    private IEnumerator LoadingBarEnumerator(AsyncOperation asynCoperation)
    {
        while (!asynCoperation.isDone)
        {
            float progress = Mathf.Clamp01(asynCoperation.progress / 0.9f);

            loadingBarSlider.value = progress;

            yield return null;
        }
    }
}