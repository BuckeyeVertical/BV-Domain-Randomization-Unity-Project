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
    private int minObjects = 200;
    // Maximum number of objects to be generated
    private int maxObjects = 300;
    private int gridRows = 5;
    private int gridColumns = 3;
    private float gridCellWidth;
    private float gridCellHeight;
    private int objectsToBePlaced;

    private Dictionary<int, List<Vector3>> gridContainers;
    private Dictionary<int, float> objectOffsets;
    private Dictionary<int, List<(Vector3 rotation, float yOffset)>> objectRotations = new Dictionary<int, List<(Vector3, float)>> 
    {
        {0, new List<(Vector3, float)> { //footballs
            (new Vector3(0, 0, 0), 0f),
            (new Vector3(0, 45, 0), 0f),
            (new Vector3(0, 90, 0), 0f),
            (new Vector3(0, 135, 0), 0f),
            (new Vector3(0, 180, 0), 0f),
            (new Vector3(0, 225, 0), 0f),
            (new Vector3(0, 270, 0), 0f),
            (new Vector3(0, 315, 0), 0f)
        }},
        {3, new List<(Vector3, float)> { //bikes
            (new Vector3(65, 40, 0), 2.9f),
            (new Vector3(-65, 90, 0), 2.9f),
            (new Vector3(78, 135, 0), 2.9f),
            //(new Vector3(-78, 180, 0), 2.9f),
            (new Vector3(25, 220, 0), 4.5f),
            (new Vector3(-25, 270, 0), 4.5f)
        }},
        {4, new List<(Vector3, float)> { //stop-sign
            (new Vector3(85, 90, 40), 0.1f),
            (new Vector3(-45, 130, 66), 0.1f),
            (new Vector3(50, 160, 12), 0.1f),
            (new Vector3(65, 45, 25), 0.1f),
            (new Vector3(-35, 10, 18), 0.1f)
        }}
    };

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
            {0,-0.27f}, // Y Offset for football
            {1,0.4f}, // Y Offset for basketball
            {2,-0.9f}, // Y Offset for baseball
            {3,4.6f}, // Y Offset for bikes
            {4,0.6f}, // Y Offset for stop sign (no pole)
        };

        //  objectRotations = new Dictionary<int, List<(Vector3 rotation, float yOffset)>> {
        //     {3, new List<(Vector3, float)> {
        //         (new Vector3(65, 0, 0), 2.9f),
        //         (new Vector3(-65, 0, 0), 2.9f),
        //         (new Vector3(78, 0, 0), 2.9f),
        //         (new Vector3(-78, 0, 0), 2.9f),
        //         (new Vector3(25, 0, 0), 5.7f),
        //         (new Vector3(-25, 0, 0), 5.7f)
        //     }},
        //     {4, new List<(Vector3, float)> {
        //         (new Vector3(85, 90, 40), 0.1f),
        //         (new Vector3(-45, 130, 66), 0.1f),
        //         (new Vector3(50, 160, 12), 0.1f),
        //         (new Vector3(65, 45, 25), 0.1f),
        //         (new Vector3(-35, 10, 18), 0.1f)
        //     }}
        // };

        // Start the coroutine to spawn objects every 5 seconds
        StartCoroutine(SpawnObjectsEveryFiveSeconds());
    }

    private IEnumerator SpawnObjectsEveryFiveSeconds()
    {
        while (true)
        {
            SpawnObjects();  // Place the objects
            yield return new WaitForSeconds(1000f);  // Wait for 5 seconds before placing again
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

        // Get rotation and y-offset if available in the dictionary
        Quaternion rotation = Quaternion.identity;
        float yOffset = objectOffsets.ContainsKey(randomIndex) ? objectOffsets[randomIndex] : 0f;
        
        if (objectRotations.ContainsKey(randomIndex))
        {
            var rotations = objectRotations[randomIndex];
            var chosenRotation = rotations[Random.Range(0, rotations.Count)];
            rotation = Quaternion.Euler(chosenRotation.rotation);
            yOffset = chosenRotation.yOffset;
        }

        spawnPosition.y += yOffset;
        GameObject newObject = Instantiate(selectedObject, spawnPosition, rotation);
        newObject.transform.parent = transform;

        gridContainers[gridIndex].Add(spawnPosition);

        ///////////
        
        // Renderer objectRenderer = newObject.GetComponent<Renderer>();
        // if (objectRenderer != null)
        // {
        //     Bounds bounds = objectRenderer.bounds;
        //     float width = bounds.size.x;
        //     float height = bounds.size.y;
        //     float depth = bounds.size.z;

        //     // Store or log bounding box information
        //     Debug.Log("Bounding Box Width: " + width);
        //     Debug.Log("Bounding Box Height: " + height);
        //     Debug.Log("Bounding Box Depth: " + depth);

        //     // You could store this data in a structure, array, or file depending on your needs
        // }
        // Debug.Log("Through");
        // // Add position to the grid container
        // gridContainers[gridIndex].Add(spawnPosition);
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