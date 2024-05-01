using System.Collections.Generic;
using DataType;
using ScriptableObjectV;
using UnityEngine;
using UnityEngine.Events;

public abstract class RawDataProcessor : MonoBehaviour
{
    [HideInInspector] public UnityEvent<List<BoundingBox3D>> onBoundingBoxProcessed;
    
    public float updateInterval = 1.0f;
    protected float LastUpdateTime;
    
    public UnityEvent<string> onRawDataProcessed = new UnityEvent<string>();
}
