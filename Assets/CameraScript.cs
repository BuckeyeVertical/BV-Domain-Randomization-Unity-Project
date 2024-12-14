using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class CameraScript : MonoBehaviour
{
    public GameObject sun;
    private Camera cam;
    private System.Random rnd;

    public GameObject objectSpawner;
    public static int picturesTaken = 0;
    
    readonly public static int totalpics = 100;
    readonly private Vector2 AspectRatio = new Vector2(1920, 1080);


    const string workingDirectory = @"D:\Data\Buckeye Vertical\Image Classifier"; 
    //const string workingDirectory = "U:\\Prelim Detection Dataset";

    public static Boolean swapPage = false;
    public static Boolean swapRoad = false;

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
        float x = rnd.Next(-60, 60);
        float y = rnd.Next(150, 200);
        float z = rnd.Next(-25, 25);
        cam.transform.localPosition = new Vector3(x, y, z);

        float rotation_y = rnd.Next(0, 359);
        float rotation_x = rnd.Next(90, 90);
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
        // Renderer rend = obj.GetComponent<Renderer>();

        // if(rend == null)
        // {
        //     Debug.LogWarning("Renderer not found on object!");
        //     return Vector2.zero;
        // }

        // Bounds b = GetComponent<Renderer>().bounds;

        Vector3 minXZ = new Vector3(b.min.x, 0, b.min.z);
        Vector3 maxXZ = new Vector3(b.max.x, 0, b.max.z);

        // Convert these modified min and max points to screen space
        Vector2 minScreenPoint = cam.WorldToScreenPoint(minXZ);
        Vector2 maxScreenPoint = cam.WorldToScreenPoint(maxXZ);

        minScreenPoint.y = AspectRatio.y - minScreenPoint.y;
        maxScreenPoint.y = AspectRatio.y - maxScreenPoint.y;

        float width = Mathf.Abs(maxScreenPoint.x - minScreenPoint.x)*1.2f;
        float height = Mathf.Abs(maxScreenPoint.y - minScreenPoint.y)*1.2f;
        return new Vector2(width,height);
    } 
    //TO-DO: Write a formula based on the rotation of the object compared to the object to make it bigger
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
            { "Basketball", 0 },
            { "Football", 1 },
            { "BMXBikeE", 2 },
            { "Sign_Stop", 3 },
            { "Baseball", 4 },
            // Add other objects as needed
        };
    }


    
    void Update()
    {
        if (picturesTaken <= totalpics){
                randomizeSun();
                randomizeCamera();
                (GameObject gameObject, Bounds bounds)[] targets = validTargets();

                if(targets.Length > 0){

                    string textToWrite = GenerateNormalizedDataString(targets);
                    string screenShotPath;
                    string filePath;

                    //Every four pictures sent to train set
                    if (picturesTaken % 5 != 0)
                    {
                        screenShotPath = workingDirectory + "\\train\\images\\" + picturesTaken.ToString() + ".png";
                        filePath = workingDirectory + "/train/labels/" + picturesTaken.ToString() + ".txt";
                    }
                    //Every fifth picture sent to validation set
                    else
                    {
                        screenShotPath = workingDirectory + "\\valid\\images\\"  + picturesTaken.ToString() + ".png";
                        filePath = workingDirectory + "/valid/labels/" + picturesTaken.ToString() + ".txt";
                    }

                    // Debug.Log(workingDirectory + "\\valid\\images\\"  + prevPicTaken.ToString() + ".png");

                    if(File.Exists(workingDirectory + "\\train\\images\\"+ prevPicTaken.ToString() + ".png") ||
                    (prevPicTaken == -1) || File.Exists(workingDirectory + "\\valid\\images\\"+ prevPicTaken.ToString() + ".png"))
                    {
                        // Create a new StreamWriter and write the text to the file
                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            writer.WriteLine(textToWrite);
                        }

                        //CHANGE TO TRAIN
                        ScreenCapture.CaptureScreenshot(screenShotPath);
                        prevPicTaken = picturesTaken;
                        prevFileCount = fileCount;
                        
                        picturesTaken++;
                        
                    }
                }
            Debug.Log(picturesTaken);
        }else{
            Debug.Log("Reached max count. Exiting application...");
            Application.Quit();

            // If you're running in the Unity Editor, stop play mode
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        
    }

    
    

    
}