using UnityEngine;
using Unity.Netcode;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene(string scene)
    {
        if (!nowWaitingForChangingScene)
        {
            DataOperator.ChangeScene(scene);
        }
    }


    bool nowWaitingForChangingScene;
    string delayedSceneName;
    public void ShutdownNetworkAndChangeScene(string scene)
    {
        if (!nowWaitingForChangingScene)
        {
            nowWaitingForChangingScene = true;
            delayedSceneName = scene;
            NetworkManager.Singleton.Shutdown();
            ChangeScene_WaitingForShuttingDownNetwork();
        }
    }
    void ChangeScene_WaitingForShuttingDownNetwork()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            Invoke(nameof(ChangeScene_WaitingForShuttingDownNetwork), 0.01f);
        }
        else
        {
            DataOperator.ChangeScene(delayedSceneName);
        }
    }
}
