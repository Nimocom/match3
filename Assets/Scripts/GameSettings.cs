using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings inst;

    public List<ColorData> colorData;

    public float distanceThreshold;

    public float elementsSwappingSpeed;

    public float delayBeforeDestroying;
    public float delayBeforeMovingUp;
    public float delayBeforeFilling;

    public float elementColorChangingSpeed;
    public float elementMovementSpeed;
    public float elementScalingSpeed;

    public float scoreTextScalingSpeed;
    public float scoreTextScalingMlt;

    public int additionalScoreAmount;

    void Awake()
    {
        inst = this;
    }
}

[System.Serializable]
public struct ColorData 
{
    public int index;
    public Color color;
}