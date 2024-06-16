using System;
using Unity.Netcode;
using UnityEngine;

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