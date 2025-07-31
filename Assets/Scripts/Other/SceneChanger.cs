using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    bool nowWaitingForChangingScene;

    public void ChangeScene(string scene)
    {
        if (!nowWaitingForChangingScene)
        {
            DataOperator.ChangeScene(scene);
        }
    }

    public void ShutdownNetworkAndChangeScene(string sceneName)
    {
        if (!nowWaitingForChangingScene)
        {
            nowWaitingForChangingScene = true;
            NetworkManager.Singleton.Shutdown();
            StartCoroutine(ChangeScene_WaitingForShuttingDownNetworkCoroutine(sceneName));
        }
    }
    IEnumerator ChangeScene_WaitingForShuttingDownNetworkCoroutine(string sceneName)
    {
        while (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        DataOperator.ChangeScene(sceneName);
    }
}
