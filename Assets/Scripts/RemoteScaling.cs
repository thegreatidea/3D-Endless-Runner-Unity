using UnityEngine;

public class PrefabController : MonoBehaviour
{
    [System.Serializable]
    public class PrefabData
    {
        public GameObject prefab;
        public Vector3 targetPosition;
        public Vector3 targetScale;
    }

    [Header("Train Prefabs")]
    public PrefabData[] trainPrefabs;

    [Header("Ground Prefabs (Position Locked)")]
    public PrefabData[] groundPrefabs;

    void Update()
    {
        // Update trains (position and scale)
        foreach (PrefabData train in trainPrefabs)
        {
            if (train.prefab != null)
            {
                train.prefab.transform.position = train.targetPosition;
                train.prefab.transform.localScale = train.targetScale;
            }
        }

        // Update grounds (only scale)
        foreach (PrefabData ground in groundPrefabs)
        {
            if (ground.prefab != null)
            {
                ground.prefab.transform.localScale = ground.targetScale;
                // Position is locked for ground prefabs
            }
        }
    }
}