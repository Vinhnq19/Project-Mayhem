using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class SceneTransitor : MonoBehaviour
{
    private static SceneTransitor instance;
    public static SceneTransitor Instance => instance;

    [SerializeField] private GameObject leftTrans;

    [SerializeField] private GameObject rightTrans;

    void Awake()
    {
        if(instance)
        {
            Destroy(this.gameObject);
        }
        instance = this;
    }

    public void OnNextScene(string sceneName)
    {

    }

    public void OnBackScene(string sceneName)
    {

    }
    
}
