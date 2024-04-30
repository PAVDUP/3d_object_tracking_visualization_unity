using System.Collections.Generic;
using DataType;
using UnityEngine;
using UnityEngine.Events;

public abstract class RawDataProcessor : MonoBehaviour
{
    public UnityEvent<List<BoundingBox3D>> onBoundingBoxProcessed;
    
    public float updateInterval = 1.0f;
    protected float LastUpdateTime;
}
