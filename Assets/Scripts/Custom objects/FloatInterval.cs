using System;

[Serializable]
public struct FloatInterval
{
    public float minValue;
    public float maxValue;

    public FloatInterval(float minValue_, float maxValue_)
    {
        minValue = minValue_;
        maxValue = maxValue_;
    }

    public float GetRandomValueFromInterval()
    {
        return UnityEngine.Random.Range(minValue, maxValue);
    }
}