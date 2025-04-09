using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Linq;

public class CameraScript : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float rotationSpeed = 70f;

    public GameObject sun;

    public GameObject objectSpawner;

    public RoadRandomizer road;

    private Camera cam;
    private System.Random rnd;

    private bool isCapturing = false;
    
    public static int picturesTaken = 0;
    
    public int totalpics = 100;
    readonly private Vector2 AspectRatio = new Vector2(1920, 1080);


    const string workingDirectory = @"D:\Data\Buckeye Vertical\Image Classifier"; 
    //const string workingDirectory = "U:\\Prelim Detection Dataset";


    private int fileCount = 0;
    private int prevFileCount = 0;

    private Dictionary<string, int> objectClassMap;
    //CHANGE THIS!!

    private int prevPicTaken = -1;

    private void randomizeSun()
    {
        // Random angle between 0 and 200 degrees.
        float angle = rnd.Next(0, 180);
        sun.transform.localRotation = Quaternion.Euler(angle, 0, 0);

        // Calculate intensity based on the angle.
        // Assuming maximum intensity is 2 and minimum is 0.5 for demonstration.
        float maxIntensity = 1.3f;
        float minIntensity = 1.0f;

        // Normalize angle for intensity calculation (making 90 degrees = 1, 0 and 180 degrees = 0)
        float normalizedAngle = Mathf.Abs(angle - 90) / 90.0f; // This will give 0 at 90 degrees and 1 at 0 or 180 degrees.

        // Calculate intensity (inverse relationship with the normalized angle).
        float intensity = maxIntensity - (normalizedAngle * (maxIntensity - minIntensity));

        // Set the sun's intensity.
        sun.GetComponent<Light>().intensity = intensity;
    }

    private void randomizeCamera()
    {
        float x = rnd.Next(-200, 200);
        float y = rnd.Next(150, 200);
        float z = rnd.Next(-50, 50);
        cam.transform.localPosition = new Vector3(x, y, z);

        float rotation_y = rnd.Next(0, 359);
        float rotation_x = rnd.Next(88, 92);
        cam.transform.localRotation = Quaternion.Euler(rotation_x, rotation_y, 0);
    }

    private (GameObject gameObject, Bounds bounds)[] validTargets()
    {
        List<(GameObject gameObject, Bounds bounds)> validTargetsList = new List<(GameObject gameObject, Bounds bounds)>();
        
        if(!(objectSpawner.transform.childCount == 0))
        {
            foreach(Transform obj in objectSpawner.transform)
            {
                GameObject go = obj.gameObject;
                Bounds go2 = CalculateCombinedBounds(go);
                if(isInView(go2))
                {
                    Debug.Log(go.name);
                    validTargetsList.Add((go, go2));
                }
            }

            
        }
        return validTargetsList.ToArray();
    }

    private bool isInView(Bounds b)
    {
        Vector3 pointOnScreen = cam.WorldToScreenPoint(b.center);

        //Check object is not behind cam
        //Should never really happen unless camera is in bad spot
        if (pointOnScreen.z < 0)
        {
            return false;
        }

        //Check object is in field of view
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
        (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            return false;
        }
        // In case objects cover each other
        // RaycastHit hit;
        // Vector3 heading = toCheck.transform.position - origin.transform.position;
        // Vector3 direction = heading.normalized;// / heading.magnitude;
        
        // if (Physics.Linecast(cam.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, out hit))
        // {
        //     if (hit.transform.name != toCheck.name)
        //     {
        //         /* -->
        //         Debug.DrawLine(cam.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, Color.red);
        //         Debug.LogError(toCheck.name + " occluded by " + hit.transform.name);
        //         */
        //         Debug.Log(toCheck.name + " occluded by " + hit.transform.name);
        //         return false;
        //     }
        // }
        return true;
    }



    //Returns a string comprised of all payload object's class, normalized x and y value, and normalized width and height
    private string GenerateNormalizedDataString((GameObject gameObject, Bounds bounds)[] validObjects)
    {
        string dataString = "";

        foreach (var (obj, b) in validObjects)
        {
            Vector2 centerPos = GetCenterPosition(b);
            Vector2 widthHeight = GetWidthHeight(b);

            dataString += 
                        GetObjectClass(obj).ToString() + " " +
                        normalize(centerPos.x, AspectRatio.x).ToString() + " " +
                        normalize(centerPos.y, AspectRatio.y).ToString() + " " +
                        normalize(widthHeight.x, AspectRatio.x).ToString() + " " +
                        normalize(widthHeight.y, AspectRatio.y).ToString() + "\n";
        }

        return dataString;
    }


    private Vector2 GetCenterPosition(Bounds b)
    {
        // Vector2 temp = cam.WorldToScreenPoint(obj.transform.position);
        // Vector2 centerPos = new Vector2(temp.x, AspectRatio.y - temp.y);
        // Renderer renderer = obj.GetComponent<Renderer>();
        // Bounds b = renderer.bounds;
        Vector2 center = cam.WorldToScreenPoint(b.center);
        float x = (float)center.x;
        float y = AspectRatio.y - center.y;
        return new Vector2(x,y);
    }
    //Returns width and height of a  gameobject as a Vector2 Object
    private Vector2 GetWidthHeight(Bounds b)
    {
        // Initialize an array to hold the 8 corners of the bounds
        Vector3[] worldCorners = new Vector3[8];

        // Get all 8 corners of the bounds in world space
        worldCorners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
        worldCorners[1] = new Vector3(b.max.x, b.min.y, b.min.z);
        worldCorners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
        worldCorners[3] = new Vector3(b.max.x, b.max.y, b.min.z);
        worldCorners[4] = new Vector3(b.min.x, b.min.y, b.max.z);
        worldCorners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
        worldCorners[6] = new Vector3(b.min.x, b.max.y, b.max.z);
        worldCorners[7] = new Vector3(b.max.x, b.max.y, b.max.z);

        // Convert each corner to screen space
        Vector2[] screenCorners = new Vector2[8];
        for (int i = 0; i < worldCorners.Length; i++)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(worldCorners[i]);
            screenPoint.y = AspectRatio.y - screenPoint.y; // Adjust for screen origin
            screenCorners[i] = new Vector2(screenPoint.x, screenPoint.y);
        }

        // Find the min and max points in screen space
        float minX = screenCorners[0].x;
        float minY = screenCorners[0].y;
        float maxX = screenCorners[0].x;
        float maxY = screenCorners[0].y;

        for (int i = 1; i < screenCorners.Length; i++)
        {
            minX = Mathf.Min(minX, screenCorners[i].x);
            minY = Mathf.Min(minY, screenCorners[i].y);
            maxX = Mathf.Max(maxX, screenCorners[i].x);
            maxY = Mathf.Max(maxY, screenCorners[i].y);
        }

        // Calculate width and height in screen space
        float width = Mathf.Abs(maxX - minX) * 1.2f; // Add padding
        float height = Mathf.Abs(maxY - minY) * 1.2f;

        return new Vector2(width, height);
    }

    private Bounds CalculateCombinedBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if(renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        Bounds combinedBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        return combinedBounds;
        
    }
    //Normalize method
    public float normalize(float value, float total)
    {
        return value / total;
    }

    public int GetObjectClass(GameObject obj)
    {
        if (obj == null) return -1; // Return -1 for invalid objects

        // Strip the "(Clone)" suffix from the object name
        string objectName = obj.name.Replace("(Clone)", "").Trim();

        // Look up the name in the dictionary
        if (objectClassMap.TryGetValue(objectName, out int classNumber))
        {
            return classNumber;
        }
        else
        {
            Debug.LogWarning($"Object name '{objectName}' not found in the dictionary.");
            return -1; // Return -1 if the name is not found
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rnd = new System.Random();
        cam = GetComponent<Camera>();
        // Initialize the dictionary with mappings
        objectClassMap = new Dictionary<string, int>
        {
            { "Football", 0 },
            { "Soccerball", 0 },
            { "Basketball", 0 },
            { "Volleyball", 0 },
            { "Sign_Stop", 1 },
            { "ScaleCar", 2 },
            // { "(Prb)Clock", 6 },
            { "Closed Umbrella", 3 },
            { "Open Umbrella", 3 },
            { "Tennis Racket", 4 },
            // { "Mob_Baseball_Bat_10", 7 },
            { "TravelSet_Suitcase", 5 },
            { "TravelSet_Suitcase Medium", 5 },
            { "Airplane", 6 },
            { "Motorcycle", 7 },
            { "mannequin", 8 },
            { "person", 9 },
            { "School Bus", 10 },
            { "Bat", 11 },
            { "Boat", 12 },
            { "Skis", 13 },
            { "Snowboard", 14 },
            { "Bed-Frame", 15 },
            { "Bed", 15 },
            
        };
    }


    void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        // Forward/backward
        if (Input.GetKey(KeyCode.UpArrow)) direction += transform.forward;
        if (Input.GetKey(KeyCode.DownArrow)) direction -= transform.forward;

        // Left/right
        if (Input.GetKey(KeyCode.LeftArrow)) direction -= transform.right;
        if (Input.GetKey(KeyCode.RightArrow)) direction += transform.right;

        // Up/down
        if (Input.GetKey(KeyCode.O)) direction += transform.up;
        if (Input.GetKey(KeyCode.P)) direction -= transform.up;

        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }

    void HandleRotation()
    {
        float pitch = 0f; // x-axis
        float yaw = 0f;   // y-axis
        float roll = 0f;  // z-axis

        // Pitch
        if (Input.GetKey(KeyCode.W)) pitch = 1f;
        if (Input.GetKey(KeyCode.S)) pitch = -1f;

        // Yaw
        if (Input.GetKey(KeyCode.A)) yaw = -1f;
        if (Input.GetKey(KeyCode.D)) yaw = 1f;

        // Roll
        if (Input.GetKey(KeyCode.Q)) roll = 1f;
        if (Input.GetKey(KeyCode.E)) roll = -1f;

        Vector3 rotation = new Vector3(pitch, yaw, roll) * rotationSpeed * Time.deltaTime;
        transform.Rotate(rotation, Space.Self);
    }


    void Update()
    {
        HandleMovement();
        HandleRotation();
        // if (picturesTaken <= totalpics)
        // {
        //     if (!isCapturing) // Only proceed if not already capturing
        //     {
        //         isCapturing = true; // Prevent new captures until finished
                
        //         // Apply new materials (road and environment)
        //         if (road != null)
        //         {
        //             // Debug.Log("road changed");
        //             // road.ApplyMaterial();
        //         }
        //         // randomizeSun();
        //         // randomizeCamera();
                
                
        //         // Find valid targets
        //         (GameObject gameObject, Bounds bounds)[] targets = validTargets();
        //         if (targets.Length > 0)
        //         {
                    
        //             StartCoroutine(CaptureFrame(targets));
        //         }
        //         else
        //         {
        //             isCapturing = false; // Release flag if no targets found
        //         }
        //     }
        // }
        // else
        // {
        //     Debug.Log("Reached max count. Exiting application...");
        //     Application.Quit();

        //     #if UNITY_EDITOR
        //         UnityEditor.EditorApplication.isPlaying = false;
        //     #endif
        // }
    }

    private IEnumerator CaptureFrame((GameObject, Bounds)[] targets)
{
    string textToWrite = GenerateNormalizedDataString(targets);
    string screenShotPath;
    string filePath;

    if (picturesTaken % 5 != 0)
    {
        screenShotPath = workingDirectory + "\\train\\images\\" + picturesTaken.ToString() + ".png";
        filePath = workingDirectory + "/train/labels/" + picturesTaken.ToString() + ".txt";
    }
    else
    {
        screenShotPath = workingDirectory + "\\valid\\images\\" + picturesTaken.ToString() + ".png";
        filePath = workingDirectory + "/valid/labels/" + picturesTaken.ToString() + ".txt";
    }

    // Write metadata to file
    using (StreamWriter writer = new StreamWriter(filePath))
    {
        // writer.WriteLine(textToWrite);
    }

    // Capture the screenshot at the end of the frame
    yield return new WaitForEndOfFrame();
    // ScreenCapture.CaptureScreenshot(screenShotPath);

    // Wait an extra frame to ensure completion
    yield return null;

    picturesTaken++;
    prevPicTaken = picturesTaken;
    isCapturing = false; // Release flag
}


    
    

    
}