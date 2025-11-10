using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public void Next()
    {
        SoundManager.Instance.PlaySfx(3);
        SceneTransitor.Instance.OnNextScene();
    }

    public void Back()
    {
        SoundManager.Instance.PlaySfx(3);
        SceneTransitor.Instance.OnBackScene();
    }
}
