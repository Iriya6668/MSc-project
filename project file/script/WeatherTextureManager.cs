using UnityEngine;
using System.Collections.Generic;

public class WeatherTextureManager : MonoBehaviour
{
    [System.Serializable]
    public class TagMaterials
    {
        // object tag 
        public string tag;
        // weather materials
        public Material sunnyMaterial;
        public Material snowyMaterial;
        public Material rainyMaterial;
    }

    public List<TagMaterials> tagMaterialsList = new List<TagMaterials>();

    private Dictionary<string, Dictionary<Renderer, Material>> originalMaterials = new Dictionary<string, Dictionary<Renderer, Material>>();

    void Start()
    {
        CacheOriginalMaterials();
    }

    void CacheOriginalMaterials()
    {
        // get the original material and save
        foreach (var tagMaterials in tagMaterialsList)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tagMaterials.tag);
            var tagOriginalMaterials = new Dictionary<Renderer, Material>();

            foreach (GameObject obj in objects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && !tagOriginalMaterials.ContainsKey(renderer))
                {
                    tagOriginalMaterials[renderer] = renderer.sharedMaterial;
                    Debug.Log($"Cached original material for {obj.name} with tag {tagMaterials.tag}");
                }
            }

            originalMaterials[tagMaterials.tag] = tagOriginalMaterials;
        }
    }

    public void ApplySunnyMaterials()
    {
        Debug.Log("Applying sunny materials");
        ApplyMaterials("sunnyMaterial");
    }

    public void ApplySnowyMaterials()
    {
        Debug.Log("Applying snowy materials");
        ApplyMaterials("snowyMaterial");
    }

    public void ApplyRainyMaterials()
    {
        Debug.Log("Applying rainy materials");
        ApplyMaterials("rainyMaterial");
    }

    public void RestoreOriginalMaterials()
    {
        Debug.Log("Restoring original materials");
        foreach (var tagEntry in originalMaterials)
        {
            foreach (var entry in tagEntry.Value)
            {
                if (entry.Key != null)
                {
                    entry.Key.sharedMaterial = entry.Value;
                    Debug.Log($"Restored original material for {entry.Key.gameObject.name} with tag {tagEntry.Key}");
                }
            }
        }
    }

    void ApplyMaterials(string materialType)
    {
        foreach (var tagMaterials in tagMaterialsList)
        {
            Material materialToApply = null;
            switch (materialType)
            {
                case "sunnyMaterial":
                    materialToApply = tagMaterials.sunnyMaterial;
                    break;
                case "snowyMaterial":
                    materialToApply = tagMaterials.snowyMaterial;
                    break;
                case "rainyMaterial":
                    materialToApply = tagMaterials.rainyMaterial;
                    break;
            }

            if (materialToApply != null)
            {
                GameObject[] objects = GameObject.FindGameObjectsWithTag(tagMaterials.tag);
                foreach (GameObject obj in objects)
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = materialToApply;
                        Debug.Log($"Applied {materialType} to {obj.name} with tag {tagMaterials.tag}");
                    }
                }
            }
        }
    }
}
