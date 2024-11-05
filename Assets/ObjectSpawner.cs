using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // Scene Objects. Any Objects we want to be randomly spawned in.
    public GameObject[] sceneObjects;
    // Road object. Used mainly to get field where objects may be placed.
    public GameObject road;
    // Gets size of the road in Vector3 coordinates.
    private Vector3 roadSize;
    // Minimum number of objects to be generated
    private int minObjects = 20;
    // Maximum number of objects to be generated
    private int maxObjects = 30;
    private int gridRows = 5;
    private int gridColumns = 3;
    private float gridCellWidth;
    private float gridCellHeight;
    private int objectsToBePlaced;

    private Dictionary<int, List<Vector3>> gridContainers;
    private Dictionary<int, float> objectOffsets;
    void Start()
    {
        roadSize = road.GetComponent<Renderer>().bounds.size;
        // Reduce the X and Z sizes by 3 units on each side to avoid objects on edges of road
        roadSize.x -= 6f;
        roadSize.z -= 6f;
        objectsToBePlaced = Random.Range(minObjects, maxObjects);
        gridCellWidth = roadSize.x / gridColumns;
        gridCellHeight = roadSize.z / gridRows;
        gridContainers = new Dictionary<int, List<Vector3>>();
        for (int i = 0; i < gridRows * gridColumns; i++)
        {
            gridContainers[i] = new List<Vector3>();
        }
        objectOffsets = new Dictionary<int, float>{
            {0,0.6f},
            {1,0.6f},
            {2,0.6f},
            {3,4.6f}, // Y Offset for bikes
            // {4,0.6f}
        };
        // Start the coroutine to spawn objects every 5 seconds
        StartCoroutine(SpawnObjectsEveryFiveSeconds());
    }

    private IEnumerator SpawnObjectsEveryFiveSeconds()
    {
        while (true)
        {
            SpawnObjects();  // Place the objects
            yield return new WaitForSeconds(5f);  // Wait for 5 seconds before placing again
        }
    }

    private void SpawnObjects()
    {
        // Clear previous objects and grid containers
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        gridContainers.Clear();

        // Reset the grid containers for new objects
        for (int i = 0; i < gridRows * gridColumns; i++)
        {
            gridContainers[i] = new List<Vector3>();
        }

        // Spawn new objects
        for (int i = 0; i < objectsToBePlaced; i++)
        {
            PlaceObjectInGrid();
        }
    }

    private void PlaceObjectInGrid()
    {
        int randomIndex = Random.Range(0, sceneObjects.Length);
        GameObject selectedObject = sceneObjects[randomIndex];
        int gridIndex = Random.Range(0, gridContainers.Count);

        Vector3 gridCenter = GetGridCenter(gridIndex);
        Vector3 spawnPosition = GetRandomPositionWithinGrid(gridCenter);

        // Check to see if there are already objects in grid
        bool overlaps = true;
        while (overlaps)
        {
            overlaps = false; // Assume no overlap initially

            if (gridContainers[gridIndex].Count > 0)
            {
                // Check for overlaps within the grid container
                foreach (Vector3 placedPosition in gridContainers[gridIndex])
                {
                    if (Vector3.Distance(spawnPosition, placedPosition) < 10f) // Adjust overlap threshold
                    {
                        overlaps = true;
                        break;
                    }
                }

                // If overlapping, regenerate the spawn position within the same grid
                if (overlaps)
                {
                    spawnPosition = GetRandomPositionWithinGrid(gridCenter);
                }
            }
        }
        spawnPosition.y += objectOffsets[randomIndex]; // Applied Y-offset

        // Place object at the valid, non-overlapping position
        GameObject newObject = Instantiate(selectedObject, spawnPosition, Quaternion.identity);
        newObject.transform.parent = transform;  // Set as child to maintain hierarchy

        Renderer objectRenderer = newObject.GetComponent<Renderer>();
        
        Debug.Log("Through");
        // Add position to the grid container
        gridContainers[gridIndex].Add(spawnPosition);
    }

    private Vector3 GetGridCenter(int gridIndex)
    {
        int row = gridIndex / gridColumns;
        int col = gridIndex % gridColumns;

        float xPos = road.transform.position.x - (roadSize.x / 2) + (col * gridCellWidth) + (gridCellWidth / 2);
        float zPos = road.transform.position.z - (roadSize.z / 2) + (row * gridCellHeight) + (gridCellHeight / 2);

        return new Vector3(xPos, road.transform.position.y + 1.5f, zPos);
    }

    private Vector3 GetRandomPositionWithinGrid(Vector3 gridCenter)
    {
        float randomOffsetX = Random.Range(-gridCellWidth / 2, gridCellWidth / 2);
        float randomOffsetZ = Random.Range(-gridCellHeight / 2, gridCellHeight / 2);

        return new Vector3(gridCenter.x + randomOffsetX, gridCenter.y, gridCenter.z + randomOffsetZ);
    }
}
