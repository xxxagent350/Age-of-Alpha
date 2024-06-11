using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformByAlphaGames : NetworkBehaviour
{
    [Header("Ќастройка")]
    [Tooltip("Network variable - с помощью сетевой переменной, rpc - с помощью сообщений(рекомендуетс€ дл€ моментальной синхронизации), interpolate - более плавна€ синхронизаци€ за счЄт искуственной задержки")]
    [SerializeField] NetworkVariable<TransformSyncTypes> syncType;
    [Tooltip("Ќа сколько кадров(fixed update) будет оставать анимаци€ от сервера (дл€ interpolate), больше = плавнее")]
    [SerializeField] uint framesBuffering = 2;
    [Tooltip("„ем больше этот параметр, тем более агрессивно клиент будет измен€ть скорость течени€ времени чтобы выровн€ть своЄ врем€ и врем€ сервера(должен быть более 0; нужен дл€ interpolate; 1 = линейное изменение)")]
    [SerializeField] float interpolatingTimeScaleMod = 0.5f;
    [Tooltip("ћаксимальна€ длина списка кадров дл€ интерпол€ции")]
    [SerializeField] uint framesMaxBuffer = 50;

    Rigidbody2D myRigidbody2D;
    NetworkObject myNetworkObject;

    private void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myNetworkObject = GetComponent<NetworkObject>();
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            myRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            myRigidbody2D.simulated = false;
        }
        if (framesBuffering == 0)
        {
            framesBuffering = 1;
            Debug.LogWarning($"framesBuffering не может быть менее 1 ({gameObject.name})");
        }
        if (interpolatingTimeScaleMod < 0)
        {
            interpolatingTimeScaleMod = 1f;
            Debug.LogWarning($"interpolatingTimeScaleMod не может быть менее 0 ({gameObject.name})");
        }
        if (framesMaxBuffer <= framesBuffering)
        {
            framesMaxBuffer = framesBuffering + 1;
            Debug.LogWarning($"framesMaxBuffer должен быть более framesBuffering ({gameObject.name})");
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton != null && myNetworkObject.IsSpawned)
        {
            if (syncType.Value == TransformSyncTypes.NetworkVariable)
            {
                NetworkVariableTransform();
            }
            if (syncType.Value == TransformSyncTypes.Rpc)
            {
                RpcTransform();
            }
            if (syncType.Value == TransformSyncTypes.Interpolate)
            {
                InterpolatingTransform();
            }
        }
    }

    readonly NetworkVariable<AlphaTransform> NetworkTransformVar = new NetworkVariable<AlphaTransform>();

    void NetworkVariableTransform()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkTransformVar.Value = new AlphaTransform(transform);
        }
        else
        {
            if (NetworkManager.Singleton.IsClient)
            {
                NetworkTransformVar.Value.SetTransformAtThis(transform);
            }
        }
    }




    void RpcTransform()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SetPositionRpc(new AlphaTransform(transform));
        }
    }

    [Rpc(SendTo.NotServer)]
    void SetPositionRpc(AlphaTransform newTransform)
    {
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            newTransform.SetTransformAtThis(transform);
        }
    }


    [Header("ќтладка")]
    [SerializeField] List<AlphaTransform> transformsInterpolating = new List<AlphaTransform>(0);
    [SerializeField] float clientsCount;

    void InterpolatingTransform()
    {
        if (NetworkManager.Singleton.IsServer) //сервер
        {
            SendTransformForInterpolatingRpc(new AlphaTransform(transform));
        }
        else
        {
            if (NetworkManager.Singleton.IsClient) //все клиенты кроме сервера
            {
                while (transformsInterpolating.Count > framesMaxBuffer)
                {
                    transformsInterpolating.RemoveAt(0);
                }
                if (transformsInterpolating.Count > 0)
                {
                    Interpolate();
                }
            }
        }
    }

    void Interpolate()
    {
        float targetPositionInOrder = transformsInterpolating.Count - 1 - framesBuffering;
        if (clientsCount >= targetPositionInOrder)
        {
            clientsCount += Mathf.Pow((transformsInterpolating.Count - 1 - clientsCount) / framesBuffering, interpolatingTimeScaleMod);
        }
        else
        {
            clientsCount += Mathf.Pow(1 + ((targetPositionInOrder - clientsCount) / framesBuffering), interpolatingTimeScaleMod);
        }
       

        if (clientsCount < 0)
        {
            clientsCount = 0;
        }
        if (clientsCount > transformsInterpolating.Count - 1)
        {
            clientsCount = transformsInterpolating.Count - 1;
        }

        AlphaTransform finalPos = new AlphaTransform();
        if (clientsCount == Mathf.RoundToInt(clientsCount))
        {
            finalPos = transformsInterpolating[Mathf.RoundToInt(clientsCount)];
        }
        else
        {
            float olderRatio = Mathf.RoundToInt(clientsCount + 0.5f) - clientsCount;
            float newerRatio = 1 - olderRatio;
            AlphaTransform olderTransform = transformsInterpolating[Mathf.RoundToInt(clientsCount - 0.5f)];
            AlphaTransform newerTransform = transformsInterpolating[Mathf.RoundToInt(clientsCount + 0.5f)];

            finalPos.position = (olderTransform.position * olderRatio) + (newerTransform.position * newerRatio);
            //finalPos.position.x = (olderTransform.position.x * olderRatio) + (newerTransform.position.x * newerRatio);
            //finalPos.position.y = (olderTransform.position.y * olderRatio) + (newerTransform.position.y * newerRatio);
            float xRot = olderTransform.rotation.eulerAngles.x + (Mathf.DeltaAngle(olderTransform.rotation.eulerAngles.x, newerTransform.rotation.eulerAngles.x) * newerRatio);
            float yRot = olderTransform.rotation.eulerAngles.y + (Mathf.DeltaAngle(olderTransform.rotation.eulerAngles.y, newerTransform.rotation.eulerAngles.y) * newerRatio);
            float zRot = olderTransform.rotation.eulerAngles.z + (Mathf.DeltaAngle(olderTransform.rotation.eulerAngles.z, newerTransform.rotation.eulerAngles.z) * newerRatio);
            finalPos.rotation.eulerAngles = new Vector3(xRot, yRot, zRot); 
        }
        finalPos.SetTransformAtThis(transform);
    }

    bool interpolationInitialized;
    [Rpc(SendTo.NotServer)]
    void SendTransformForInterpolatingRpc(AlphaTransform newTransform_)
    {
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            if (!interpolationInitialized)
            {
                for (int transform2DNum = 0; transform2DNum < framesMaxBuffer; transform2DNum++)
                {
                    transformsInterpolating.Add(newTransform_);
                }
                clientsCount = transformsInterpolating.Count - 1;
                interpolationInitialized = true;
            }
            //if (initializingFrameNum == )
            transformsInterpolating.Add(newTransform_);
            clientsCount--;
        }
    }


    public enum TransformSyncTypes
    {
        NetworkVariable,
        Rpc,
        Interpolate
    }
}



[Serializable]
public struct AlphaTransform : INetworkSerializable
{
    public Vector3 position;
    public Quaternion rotation;

    public AlphaTransform(Vector3 position_, Quaternion rotation_)
    {
        position = position_;
        rotation = rotation_;
    }
    public AlphaTransform(Transform transform_)
    {
        position = transform_.position;
        rotation = transform_.rotation;
    }
    public void SetTransformAtThis(Transform transform_)
    {
        transform_.SetPositionAndRotation(position, rotation);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
    }
}