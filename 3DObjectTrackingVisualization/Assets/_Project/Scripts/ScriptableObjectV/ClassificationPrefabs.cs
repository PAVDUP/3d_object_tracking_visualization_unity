using System;
using DataType;
using UnityEngine;

namespace ScriptableObjectV
{
    [CreateAssetMenu(fileName = "ClassificationPrefabsInfo", menuName = "3DVisualization/ClassificationPrefabs", order = 1)]
    public class ClassificationPrefabs : ScriptableObject
    {
        public ClassificationPrefab[] classificationPrefabs;
    }
    
    [Serializable]
    public class ClassificationPrefab
    {
        public BoundingBox3DType classificationType;
        public GameObject[] classificationPrefabCandidates;
    }
}
