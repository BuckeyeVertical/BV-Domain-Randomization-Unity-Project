using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // Scene Objects. Any Objects we want to be randomly spawned in.
    public GameObject[] sceneObjects;
    // Road object. Used mainly to get field where objects may be placed.
    public GameObject road;

    public ObjectMaterialRandomizer objectRandomizer;
    // Gets size of the road in Vector3 coordinates.
    private Vector3 roadSize;
    // Minimum number of objects to be generated
    public int minObjects = 100;
    // Maximum number of objects to be generated
    public int maxObjects = 200;
    private int gridRows = 10;
    private int gridColumns = 6;
    private float gridCellWidth;
    private float gridCellHeight;
    private int objectsToBePlaced;

    private Dictionary<int, List<Vector3>> gridContainers;
    private Dictionary<int, float> objectOffsets;
    private Dictionary<int, List<(Vector3 rotation, float yOffset)>> objectRotations;

    private bool isCapturingFrame = false;

    // private Dictionary<int, List<(Color color)>> objectColors;

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
            {0,1.35f}, // Y Offset for football
            {1,1.62f}, // Y Offset for Soccerball
            {2,1.95f}, // Y Offset for basketball
            {3,1.65f}, // Y Offset for basketball
            // {2,-0.9f}, // Y Offset for baseball
            // {2,4.6f}, // Y Offset for bikes
            {4,0.1f}, // Y Offset for stop sign (no pole)
            // {4,0f}, // Y Offset for Airplane
            {5,1.2f}, // Y Offset for Car
            // {6,0.2f}, // Y Offset for Clock
            {6,0.9f}, // Y Offset for Closed Umbrella
            {7,0.35f}, // Y Offset for Tennis Racket
            // {7,0.9f}, // Y Offset for Baseball Bat
            {8,1.8f}, // Y Offset for Suitcase Large
            {9,1.5f}, // Y Offset for Suitcase Medium
        };
        objectRotations = new Dictionary<int, List<(Vector3, float)>> 
        {
            {0, new List<(Vector3, float)> { //footballs
                (new Vector3(0, 0, 0), 1.35f),
                (new Vector3(0, 45, 0), 1.35f),
                (new Vector3(0, 90, 0), 1.35f),
                (new Vector3(0, 135, 0), 1.35f),
                (new Vector3(0, 180, 0), 1.35f),
                (new Vector3(0, 225, 0), 1.35f),
                (new Vector3(0, 270, 0), 1.35f),
                (new Vector3(0, 315, 0), 1.35f)
            }},
            {1, new List<(Vector3, float)> { // soccerballs 
                (new Vector3(0, 0, 0), 1.62f),
                (new Vector3(10, 45, -30), 1.62f),
                (new Vector3(-15, 90, 25), 1.62f),
                (new Vector3(20, -135, -10), 1.62f),
                (new Vector3(5, 180, 60), 1.62f),
                (new Vector3(-20, -225, 45), 1.62f),
                (new Vector3(35, 270, -15), 1.62f),
                (new Vector3(-40, 315, 20), 1.62f)
            }},
            // {2, new List<(Vector3, float)> { //bikes
            //     (new Vector3(65, 40, 0), 2.9f),
            //     (new Vector3(-65, 90, 0), 2.9f),
            //     (new Vector3(78, 135, 0), 2.9f),
            //     //(new Vector3(-78, 180, 0), 2.9f),
            //     (new Vector3(25, 220, 0), 5.7f),
            //     (new Vector3(-25, 270, 0), 5.7f)
            // }},
            {2, new List<(Vector3, float)> { // Basketballs 
                (new Vector3(0, 0, 0), 1.95f),
                (new Vector3(10, 45, -30), 1.95f),
                (new Vector3(-15, 90, 25), 1.95f),
                (new Vector3(20, -135, -10), 1.95f),
                (new Vector3(5, 180, 60), 1.95f),
                (new Vector3(-20, -225, 45), 1.95f),
                (new Vector3(35, 270, -15), 1.95f),
                (new Vector3(-40, 315, 20), 1.95f)
            }},
            {3, new List<(Vector3, float)> { // volleyballs
                (new Vector3(0, 0, 0), 1.65f),
                (new Vector3(10, 45, -30), 1.65f),
                (new Vector3(-15, 90, 25), 1.65f),
                (new Vector3(20, -135, -10), 1.65f),
                (new Vector3(5, 180, 60), 1.65f),
                (new Vector3(-20, -225, 45), 1.65f),
                (new Vector3(35, 270, -15), 1.65f),
                (new Vector3(-40, 315, 20), 1.65f)
            }},
            {4, new List<(Vector3, float)> { //Stop Sign
                (new Vector3(270, 90, 0), 0.1f),
                (new Vector3(270, 130, 0), 0.1f),
                (new Vector3(270, 160, 0), 0.1f),
                (new Vector3(270, 45, 0), 0.1f),
                
            }},
            // {4, new List<(Vector3, float)> { //airplane
            //     (new Vector3(0, 90, 0), 0.1f),
            //     (new Vector3(0, 130, 0), 0.1f),
            //     (new Vector3(0, 160, 0), 0.1f),
            //     (new Vector3(0, 45, 0), 0.1f),
            //     (new Vector3(6, 0, 180), 1.3f)
            // }},
            {5, new List<(Vector3, float)> { // Toy Car
                (new Vector3(0, 0, 0), 1.2f),
                (new Vector3(0, 90, 0), 1.2f),
                (new Vector3(0, 180, 0), 1.2f),
                (new Vector3(0, 270, 0), 1.2f),
            }},
            // {6, new List<(Vector3, float)> { // Clock
            //     (new Vector3(0, 0, 0), 0.2f),
            //     (new Vector3(0, 90, 0), 0.2f),
            //     (new Vector3(0, 180, 0), 0.2f),
            //     (new Vector3(0, 270, 0), 0.2f),
            // }},
            {6, new List<(Vector3, float)> { // Umbrella
                (new Vector3(0, 0, 90), 0.2f),
                (new Vector3(0, 90, 90), 0.2f),
                (new Vector3(0, 180, 90), 0.2f),
                (new Vector3(0, 270, 90), 0.2f),
            }},
            // {7, new List<(Vector3, float)> { // Baseball Bat
            //     (new Vector3(0, 0, 90), 0.7f),
            //     (new Vector3(0, 90, 90), 0.7f),
            //     (new Vector3(0, 180, 90), 0.7f),
            //     (new Vector3(0, 270, 90), 0.7f),
            // }},
            {7, new List<(Vector3, float)> { // Tennis Racket
                (new Vector3(90, 0, 0), 0.35f),
                (new Vector3(90, 90, 0), 0.35f),
                (new Vector3(90, 180, 0), 0.35f),
                (new Vector3(90, 270, 0), 0.35f),
                (new Vector3(90, 45, 0), 0.35f),
                (new Vector3(90, 135, 0), 0.35f),
                (new Vector3(90, 225, 0), 0.35f),
                (new Vector3(90, 315, 0), 0.35f),
                (new Vector3(90, 60, 0), 0.35f),
                (new Vector3(90, 300, 0), 0.35f)
            }},
            {8, new List<(Vector3, float)> { // Suitcase Large
                (new Vector3(0, 0, 0), 1.8f),
                (new Vector3(0, 90, 0), 1.8f),
                (new Vector3(0, 180, 0), 1.8f),
                (new Vector3(0, 270, 0), 1.8f),
            }},
            {9, new List<(Vector3, float)> { // Suitcase Medium
                (new Vector3(0, 0, 0), 1.5f),
                (new Vector3(0, 90, 0), 1.5f),
                (new Vector3(0, 180, 0), 1.5f),
                (new Vector3(0, 270, 0), 1.5f),
            }},
        };

        

        // Start the coroutine to spawn objects every 5 seconds
        StartCoroutine(SpawnObjectsEveryFiveSeconds());
    }

    private IEnumerator SpawnObjectsEveryFiveSeconds()
    {
        while (true)
        {
            // Wait until the camera is done capturing
            while (isCapturingFrame)
            {
                yield return null; // Keep waiting until the capture finishes
            }

            SpawnObjects(); // Place the objects
            yield return new WaitForSeconds(2); // Wait before running again
        }
    }

    public void SetCaptureFlag(bool value)
    {
        isCapturingFrame = value;
    }

    public void SpawnObjects()
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

        // Randomize albedo for the new object
        if (objectRandomizer != null)
        {
            objectRandomizer.RandomizeObjectAlbedo(newObject);
        }

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

        return new Vector3(xPos, road.transform.position.y, zPos);
    }

    private Vector3 GetRandomPositionWithinGrid(Vector3 gridCenter)
    {
        float randomOffsetX = Random.Range(-gridCellWidth / 2, gridCellWidth / 2);
        float randomOffsetZ = Random.Range(-gridCellHeight / 2, gridCellHeight / 2);

        return new Vector3(gridCenter.x + randomOffsetX, gridCenter.y, gridCenter.z + randomOffsetZ);
    }
}