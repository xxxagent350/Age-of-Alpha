using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct NetworkString : INetworkSerializable
{
    public char[] Symbols { get; private set; }

    public NetworkString(string inputString)
    {
        if (inputString == null)
        {
            Debug.LogWarning("Не задана inputString для NetworkString");
            Symbols = new char[0];
            return;
        }
        Symbols = new char[inputString.Length];
        for (int symbolNum = 0; symbolNum < inputString.Length; symbolNum++)
        {
            Symbols[symbolNum] = inputString[symbolNum];
        }
    }

    public string String
    {
        get
        {
            string outputString = "";
            if (Symbols == null)
            {
                Debug.LogWarning("Попытка получить не заданную string из NetworkString");
            }
            foreach (char symbol in Symbols)
            {
                outputString += symbol;
            }
            return outputString;
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Length
        int length = 0;
        if (Symbols != null && !serializer.IsReader)
        {
            length = Symbols.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            Symbols = new char[length];
        }

        if (Symbols != null)
        {
            for (int n = 0; n < length; ++n)
            {
                serializer.SerializeValue(ref Symbols[n]);
            }
        }
    }
}