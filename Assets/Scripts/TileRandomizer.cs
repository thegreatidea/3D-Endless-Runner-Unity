using UnityEngine;

public class TileRandomizer : MonoBehaviour
{
    public GameObject[] obstacleVariants;

    public void RandomizeObstacles()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Obstacle"))
            {
                child.gameObject.SetActive(Random.value > 0.5f); // 50% chance
            }
        }
    }
}