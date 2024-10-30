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
    public static int counter = 0;
    readonly public static int totalpics = 3;
    readonly private Vector2 AspectRatio = new Vector2(1920, 1080);

    const string workingDirectory = "D:\\Data\\Buckeye Vertical\\Image Classifier";
    //const string workingDirectory = "U:\\Prelim Detection Dataset";

    public static Boolean swapPage = false;
    public static Boolean swapRoad = false;

    private int fileCount = 0;
    private int prevFileCount = 0;

    private int totalObjects = 9216;
    private int fileCountConstant = 0;

    //CHANGE THIS!!
    private int numPayloads = 8;

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

    private GameObject[] validTargets()
    {
        List<GameObject> validTargetsList = new List<GameObject>();
        if(!(objectSpawner.transform.childCount == 0))
        {
            foreach(Transform obj in objectSpawner.transform)
            {
                GameObject go = obj.gameObject;
                if(isInView(go))
                {
                    validTargetsList.Add(go);
                }
            }

            
        }
        return validTargetsList.ToArray();;
    }

    private bool isInView(GameObject obj)
    {
        Vector3 pointOnScreen = cam.WorldToScreenPoint(obj.GetComponent<Renderer>().bounds.center);

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
    private string GenerateNormalizedDataString(GameObject[] validObjects)
    {
        string dataString = "";

        foreach (GameObject obj in validObjects)
        {
            Vector2 centerPos = GetCenterPosition(obj);
            Vector2 widthHeight = GetWidthHeight(obj);

            dataString += normalize(centerPos.x, AspectRatio.x).ToString() + " " +
                        normalize(centerPos.y, AspectRatio.y).ToString() + " " +
                        normalize(widthHeight.x, AspectRatio.x).ToString() + " " +
                        normalize(widthHeight.y, AspectRatio.y).ToString() + "\n";
        }

        return dataString;
    }


    private Vector2 GetCenterPosition(GameObject obj)
    {
        // Vector2 temp = cam.WorldToScreenPoint(obj.transform.position);
        // Vector2 centerPos = new Vector2(temp.x, AspectRatio.y - temp.y);
        Renderer renderer = obj.GetComponent<Renderer>();
        Bounds b = renderer.bounds;
        Vector2 center = cam.WorldToScreenPoint(b.center);
        return center;
    }
    //Returns width and height of a  gameobject as a Vector2 Object
    private Vector2 GetWidthHeight(GameObject obj)
    {
        Renderer rend = obj.GetComponent<Renderer>();

        if(rend == null)
        {
            Debug.LogWarning("Renderer not found on object!");
            return Vector2.zero;
        }

        Bounds b = GetComponent<Renderer>().bounds;

        Vector2 minScreenPoint = cam.WorldToScreenPoint(b.min);
        Vector2 maxScreenPoint = cam.WorldToScreenPoint(b.max);

        float width = Mathf.Abs(maxScreenPoint.x - minScreenPoint.x);
        float height = Mathf.Abs(maxScreenPoint.y - minScreenPoint.y);
        return new Vector2(width,height);
    }   
    //Normalize method
    public float normalize(float value, float total)
    {
        return value / total;
    }

    // Start is called before the first frame update
    void Start()
    {
        rnd = new System.Random();
        cam = GetComponent<Camera>();
    }


    
    void Update()
    {

        if (picturesTaken <= totalpics){
            if(!swapPage)
            {
                randomizeSun();
                randomizeCamera();
                GameObject[] targets = validTargets();
                if(targets.Length > 0){

                    string textToWrite = GenerateNormalizedDataString(targets);
                    string screenShotPath;
                    string filePath;
                    //Every four pictures sent to train set
                    if (picturesTaken % 5 != 0)
                    {
                        screenShotPath = workingDirectory + "\\train\\images\\" + (fileCount+fileCountConstant) + "_" + picturesTaken.ToString() + ".png";
                        filePath = workingDirectory + "\\train\\labels\\" + (fileCount+fileCountConstant) + "_" + picturesTaken.ToString() + ".txt";
                    }
                    //Every fifth picture sent to validation set
                    else
                    {
                        screenShotPath = workingDirectory + "\\valid\\images\\" + (fileCount+fileCountConstant) + "_" + picturesTaken.ToString() + ".png";
                        filePath = workingDirectory + "\\valid\\labels\\" + (fileCount+fileCountConstant) + "_" + picturesTaken.ToString() + ".txt";
                    }

                    if(File.Exists(workingDirectory + "\\train\\images\\" + (prevFileCount + fileCountConstant) + "_" + prevPicTaken.ToString() + ".png") ||
                    prevPicTaken == -1|| File.Exists(workingDirectory + "\\valid\\images\\" + (prevFileCount + fileCountConstant) + "_" + prevPicTaken.ToString() + ".png"))
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
            }
            else
            {
                swapRoad = false;
                Debug.Log(swapRoad);
                swapPage = false;
            }
        }
        else
        {
            //If the current fileCount is less than the 0-indexed final number of iterations
            if(fileCount < (totalObjects/numPayloads)-1)
            {
                fileCount++;
                swapRoad = true;
                Debug.Log(swapRoad);
                swapPage = true;
                picturesTaken = 0;
            }
            else
            {
                Debug.Log("Execution Finished");
                return;
            }
        }
    }

    
    

    
}