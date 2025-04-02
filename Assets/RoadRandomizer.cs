using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadRandomizer : MonoBehaviour
{

    int count = 0;
    List<string> materialPaths = new List<string>
{
    "BackDrop/paper",
    "BackDrop/Pavement",
    "BackDrop/Pattern2",
    "BackDrop/Asphalt_material_10"
};

    

    // Update is called once per frame
    // void Update()
    // {
    // //     Debug.Log("RoadScript called");
    //     if(CameraScript.swapRoad)
    //     {
    //         Debug.Log("Swapping Road");
    //         int randomIndex = Random.Range(0, materialPaths.Length);
    //         ApplyMaterial(randomIndex);
    //     }
    // }

    public void ApplyMaterial(){
        Material yourMaterial = Resources.Load(materialPaths[count], typeof(Material)) as Material;
        this.gameObject.GetComponent<Renderer>().material = yourMaterial;
        count += 1;
        if (count > 3){
            count = 0;
        }
        //Debug.Log("Road Changed 2");
    }
}
