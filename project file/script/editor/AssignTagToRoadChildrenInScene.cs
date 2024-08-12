using UnityEditor;
using UnityEngine;

public class AssignTagToRoadChildrenInScene : EditorWindow
{
    private GameObject prefab;
    private string tagName = "Main Road 1"; // Default tag name

    [MenuItem("Tools/Assign Tag to Road Children in Scene")]
    public static void ShowWindow()
    {
        GetWindow<AssignTagToRoadChildrenInScene>("Assign Tag to Road Children");
    }

    private void OnGUI()
    {
        GUILayout.Label("Assign Tag to Road Children", EditorStyles.boldLabel);

        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), true);
        tagName = EditorGUILayout.TextField("Tag Name", tagName);

        if (GUILayout.Button("Assign Tag"))
        {
            if (prefab != null && !string.IsNullOrEmpty(tagName))
            {
                AssignTagToRoadChildren(prefab, tagName);
            }
            else
            {
                Debug.LogWarning("Please specify a Prefab object and a Tag name.");
            }
        }
    }

    private static void AssignTagToRoadChildren(GameObject prefab, string tagName)
    {
        Debug.Log("Checking and creating Tag if necessary");

        // Check and create Tag
        CheckAndCreateTag(tagName);

        Debug.Log("Starting to iterate over the Prefab object");

        // Iterate over the specified Prefab object
        AssignTagToRoadChildrenRecursive(prefab.transform, tagName);

        Debug.Log("Tag has been assigned to all road children in the Prefab.");
    }

    private static void AssignTagToRoadChildrenRecursive(Transform parent, string tagName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("S"))
            {
                Debug.Log($"Found major segment object: {child.name}");

                foreach (Transform segment in child)
                {
                    if (segment.name.StartsWith("R"))
                    {
                        Debug.Log($"Found minor segment object: {segment.name}");

                        var road = segment.Find("R1.0/road");
                        if (road != null)
                        {
                            Debug.Log($"Found road object: {road.name}");
                            foreach (Transform roadChild in road)
                            {
                                Debug.Log($"Assigning Tag '{tagName}' to object: {roadChild.name}");
                                roadChild.tag = tagName;
                            }
                        }
                    }
                }
            }

            // Recursively check child objects
            AssignTagToRoadChildrenRecursive(child, tagName);
        }
    }

    private static void CheckAndCreateTag(string tag)
    {
        // Check if Tag exists
        if (!TagExists(tag))
        {
            // If Tag does not exist, create a new Tag
            Debug.Log($"Tag '{tag}' does not exist, creating a new Tag");
            AddTag(tag);
        }
        else
        {
            Debug.Log($"Tag '{tag}' already exists");
        }
    }

    private static bool TagExists(string tag)
    {
        foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (existingTag.Equals(tag))
            {
                return true;
            }
        }
        return false;
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Ensure Tag is not duplicated
        bool tagExists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag)) { tagExists = true; break; }
        }

        // Add a new Tag
        if (!tagExists)
        {
            Debug.Log($"Adding new Tag: {tag}");
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTag.stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }
}
