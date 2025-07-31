using System;
using Unity.Netcode;

[Serializable]
public struct Ship : INetworkSerializable
{
    public uint shipPrefabNum;
    public ModuleOnShipData[] modulesOnShipData;

    public Ship(uint newShipPrefabNum, ModuleOnShipData[] newModulesOnShipData)
    {
        shipPrefabNum = newShipPrefabNum;
        modulesOnShipData = newModulesOnShipData;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref shipPrefabNum);

        // Length
        int length = 0;
        if (!serializer.IsReader)
        {
            length = modulesOnShipData.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            modulesOnShipData = new ModuleOnShipData[length];
        }

        for (int n = 0; n < length; n++)
        {
            serializer.SerializeValue(ref modulesOnShipData[n]);
        }
    }
}
