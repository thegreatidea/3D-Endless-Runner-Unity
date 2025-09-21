using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject startTilePrefab;         // The first tile in the game
    public GameObject[] tilePrefabs;           // Tiles for endless loop (excluding start tile)
    public float tileLength = 28f;             // Length of one tile
    public int numberOfTilesOnScreen = 5;      // Number of tiles visible at once
    public Transform playerTransform;          // Reference to player
    [SerializeField] private float tileRecycleOffset = 35f; // Distance before recycling tiles
    public float startZ = 0f; // Add this line to set the starting Z position

    [Header("Pooling")]
    private Queue<GameObject> tilePool = new Queue<GameObject>();
    private List<GameObject> activeTiles = new List<GameObject>();
    private float zSpawn;

    void Start()
    {
        zSpawn = startZ; // Set initial spawn position

        // Preload tiles into pool
        int initialPoolSize = numberOfTilesOnScreen + 2;
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject prefabToUse = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
            GameObject tile = Instantiate(prefabToUse, Vector3.zero, Quaternion.identity);
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }

        // Spawn the first tile with the special prefab
        GameObject firstTile = Instantiate(startTilePrefab, Vector3.forward * zSpawn, Quaternion.identity);
        activeTiles.Add(firstTile);
        zSpawn += tileLength;

        // Spawn rest from pool
        for (int i = 1; i < numberOfTilesOnScreen; i++)
        {
            SpawnTileFromPool();
        }
    }

    void Update()
    {
        // When player moves ahead enough, spawn new tile and remove old one
        if (playerTransform.position.z - tileRecycleOffset > zSpawn - (numberOfTilesOnScreen * tileLength))
        {
            SpawnTileFromPool();
            RecycleOldestTile();
        }
    }
  
    void SpawnTileFromPool()
    {
        // If pool is empty, refill it
        if (tilePool.Count == 0)
        {
            GameObject refill = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Length)], Vector3.zero, Quaternion.identity);
            refill.SetActive(false);
            tilePool.Enqueue(refill);
        }

        GameObject tile = tilePool.Dequeue();
        tile.transform.position = Vector3.forward * zSpawn;
        tile.transform.rotation = Quaternion.identity;
        tile.SetActive(true);

        activeTiles.Add(tile);
        zSpawn += tileLength;
    }

    void RecycleOldestTile()
    {
        GameObject tileToRecycle = activeTiles[0];

        // If this is the start tile, destroy it instead of pooling
        if (tileToRecycle == activeTiles[0] && tileToRecycle == startTilePrefab || tileToRecycle.name.Contains(startTilePrefab.name))
        {
            Destroy(tileToRecycle);
        }
        else
        {
            tileToRecycle.SetActive(false);
            tilePool.Enqueue(tileToRecycle);
        }
        activeTiles.RemoveAt(0);
    }
}
