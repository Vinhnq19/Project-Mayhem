using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public void Next()
    {
        SceneTransitor.Instance.OnNextScene();
    }
    
    public void Back()
    {
        SceneTransitor.Instance.OnBackScene();
    }
}
