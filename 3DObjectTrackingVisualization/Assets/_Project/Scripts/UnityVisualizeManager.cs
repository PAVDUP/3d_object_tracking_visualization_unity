using System;
using System.Collections;
using System.Collections.Generic;
using DataType;
using UnityEngine;
using Utils;
using VisualizeModule;

/// <summary>
/// Facade for visualizing data in Unity.
/// </summary>
public class UnityVisualizeManager : Singleton<UnityVisualizeManager>
{
    // 0. Raw Data Processor
    [Header("Raw Data Processor")] public RawDataProcessor rawDataProcessor;
    
    // 1. Data
    [Space(10)][Header("Data")]
    private List<BoundingBox3D> _currentBoundingBoxes = new List<BoundingBox3D>();

    // 2. Modules (Visualizers)
    [Space(10)][Header("Modules")]
    public BoundingBox3DVisualizer boundingBox3DVisualizer;


    protected override void Awake()
    {
        base.Awake();
        
        if (rawDataProcessor == null)
        {
            Debug.LogError("[UnityVisualizeDataStore] RawDataProcessor is not set.");
        }
        
        rawDataProcessor.onBoundingBoxProcessed.AddListener(UpdateBoundingBoxes);
    }

    private void UpdateBoundingBoxes(List<BoundingBox3D> boundingBoxes)
    {
        _currentBoundingBoxes = boundingBoxes;
        boundingBox3DVisualizer.VisualizeBoundingBoxes(_currentBoundingBoxes);
    }
}
