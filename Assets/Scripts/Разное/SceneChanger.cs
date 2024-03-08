using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene(Object scene)
    {
        SceneManager.LoadScene(scene.name);
    }
}
