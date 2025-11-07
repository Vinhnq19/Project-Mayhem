using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.PackageManager.UI;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitor : MonoBehaviour
{
    private static SceneTransitor instance;
    public static SceneTransitor Instance => instance;

    public List<string> scenes;
    private int _current = 0;


    [SerializeField] private RectTransform transitionScreen;


    void Awake()
    {
        if (instance)
        {
            Destroy(this.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {

    }

    public void OnNextScene()
    {
        StartCoroutine(DoTransition(scenes[++_current]));
    }

    public void OnBackScene()
    {
        StartCoroutine(DoTransition(scenes[--_current]));
    }

    private IEnumerator DoTransition(string sceneName)
    {
        float screenWidth = transitionScreen.rect.width;
        transitionScreen.gameObject.SetActive(true);
        transitionScreen.anchoredPosition = new Vector2(-screenWidth, 0);

        // Slide vào giữa (từ trái → trung tâm)
        yield return transitionScreen.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

        // Load Scene
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Slide ra bên phải (trung tâm → phải)
        yield return transitionScreen.DOAnchorPos(new Vector2(screenWidth, 0), 0.5f).SetEase(Ease.InQuad).WaitForCompletion();

        // Reset lại vị trí chờ lần chuyển tiếp sau (đưa lại bên trái)
        transitionScreen.anchoredPosition = new Vector2(-screenWidth, 0);
        transitionScreen.gameObject.SetActive(false);
    }

}
