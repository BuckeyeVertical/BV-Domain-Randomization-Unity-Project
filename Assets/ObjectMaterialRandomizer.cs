using UnityEngine;

public class ObjectMaterialRandomizer : MonoBehaviour
{
    /// <summary>
    /// Randomizes the albedo color of the material for all Renderers in the given GameObject and its children.
    /// </summary>
    /// <param name="targetObject">The GameObject to search through.</param>
    public void RandomizeObjectAlbedo(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Target object is null. Cannot randomize albedo.");
            return;
        }

        // Randomize the material's albedo in the parent object and its children
        RandomizeAlbedoRecursively(targetObject);
    }

    private void RandomizeAlbedoRecursively(GameObject obj)
    {
        // Check if the current object has a Renderer
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Iterate through each material in the renderer
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    // Assign a random albedo color
                    material.color = new Color(Random.value, Random.value, Random.value);
                }
            }
        }

        // Recursively call for all child objects
        foreach (Transform child in obj.transform)
        {
            RandomizeAlbedoRecursively(child.gameObject);
        }
    }
}
