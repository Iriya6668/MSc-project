using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WeatherEditorWindow : EditorWindow
{
    private int selectedWeatherIndex = 0;
    private string[] weatherOptions = { "Sunny", "Rainy", "Snowy" };

    // weather skybox materials
    public Material sunnySkyboxMaterial;
    public Material rainySkyboxMaterial;
    public Material snowySkyboxMaterial;
    public Renderer skyboxRenderer;

    // weather effect array
    public GameObject[] sunnyEffects;
    public GameObject[] rainyEffects;
    public GameObject[] snowyEffects;

    private Vector2 scrollPosition;

    // weather effect controller array
    public GameObject[] rainEffectManagerGameObjects;
    public GameObject[] snowEffectManagerGameObjects;
    public GameObject[] weatherTextureManagerGameObjects;
    private WeatherTextureManager weatherTextureManager;

    private const string EditorPrefsKey = "WeatherEditorWindow";

    private bool showSceneConfig = true;
    private bool showMaterialsDetailConfig = false;
    private bool showEffectsDetailConfig = false;
    private bool disableCurrentWeatherEffects = false;

    private Dictionary<string, bool> materialFoldoutStates = new Dictionary<string, bool>();
    private Dictionary<GameObject, bool> effectFoldoutStates = new Dictionary<GameObject, bool>();

    private readonly string[] unwantedProperties = new string[]
    {
        "_QueueOffset", "_QueueControl", "unity_Lightmaps", "unity_LightProbes", "unity_ReflectionProbes", "unity_ShadowMasks", "unity_LightmapsInd", "unity_LightmapsInd"
    };

    public WeatherParameterChanger weatherParameterChanger; // Reference to WeatherParameterChanger

    [MenuItem("Window/Weather Editor")]
    public static void ShowWindow()
    {
        GetWindow<WeatherEditorWindow>("Weather Editor");
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Weather Settings", EditorStyles.boldLabel);

        // Weather selection dropdown
        selectedWeatherIndex = EditorGUILayout.Popup("Select Weather", selectedWeatherIndex, weatherOptions);

        // Scene Configuration
        showSceneConfig = EditorGUILayout.Foldout(showSceneConfig, "Scene Configuration");
        if (showSceneConfig)
        {
            // Scene Renderer
            GUILayout.Label("Scene Renderer", EditorStyles.boldLabel);
            skyboxRenderer = (Renderer)EditorGUILayout.ObjectField("Skybox Renderer", skyboxRenderer, typeof(Renderer), true);

            // Skybox Materials Configuration
            GUILayout.Label("Skybox Materials", EditorStyles.boldLabel);
            DisplaySkyboxMaterials();

            // Effects Configuration
            GUILayout.Label("Effects Configuration", EditorStyles.boldLabel);
            GUILayout.Label("Sunny Effects", EditorStyles.boldLabel);
            DisplayEffectArray(ref sunnyEffects, "SunnyEffects");

            GUILayout.Label("Rainy Effects", EditorStyles.boldLabel);
            DisplayEffectArray(ref rainyEffects, "RainyEffects");

            GUILayout.Label("Snowy Effects", EditorStyles.boldLabel);
            DisplayEffectArray(ref snowyEffects, "SnowyEffects");

            // Rain Effect Manager GameObjects
            GUILayout.Label("Rain Effect Managers", EditorStyles.boldLabel);
            DisplayEffectManagerArray(ref rainEffectManagerGameObjects, "RainEffectManagers");

            // Snow Effect Manager GameObjects
            GUILayout.Label("Snow Effect Managers", EditorStyles.boldLabel);
            DisplayEffectManagerArray(ref snowEffectManagerGameObjects, "SnowEffectManagers");

            // Weather Texture Manager GameObjects
            GUILayout.Label("Weather Texture Managers", EditorStyles.boldLabel);
            DisplayEffectManagerArray(ref weatherTextureManagerGameObjects, "WeatherTextureManagers");

            // Weather Parameter Changer
            GUILayout.Label("Weather Parameter Changer", EditorStyles.boldLabel);
            weatherParameterChanger = (WeatherParameterChanger)EditorGUILayout.ObjectField("Weather Parameter Changer", weatherParameterChanger, typeof(WeatherParameterChanger), true);

            // Option to disable current weather effects
            disableCurrentWeatherEffects = EditorGUILayout.Toggle("Disable Current Weather Effects", disableCurrentWeatherEffects);
        }

        // Apply button
        if (GUILayout.Button("Apply Weather"))
        {
            ApplyWeather(selectedWeatherIndex);
        }

        // Material Detail Configuration
        showMaterialsDetailConfig = EditorGUILayout.Foldout(showMaterialsDetailConfig, "Material Detail Configuration");
        if (showMaterialsDetailConfig)
        {
            DisplayAllTagMaterialDetails();
        }

        // Effects Detail Configuration
        showEffectsDetailConfig = EditorGUILayout.Foldout(showEffectsDetailConfig, "Effects Detail Configuration");
        if (showEffectsDetailConfig)
        {
            DisplayEffectDetails();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DisplaySkyboxMaterials()
    {
        switch (selectedWeatherIndex)
        {
            case 0:
                sunnySkyboxMaterial = (Material)EditorGUILayout.ObjectField("Sunny Skybox Material", sunnySkyboxMaterial, typeof(Material), false);
                break;
            case 1:
                rainySkyboxMaterial = (Material)EditorGUILayout.ObjectField("Rainy Skybox Material", rainySkyboxMaterial, typeof(Material), false);
                break;
            case 2:
                snowySkyboxMaterial = (Material)EditorGUILayout.ObjectField("Snowy Skybox Material", snowySkyboxMaterial, typeof(Material), false);
                break;
        }
    }

    private void DisplayEffectArray(ref GameObject[] effects, string prefsKey)
    {
        int newSize = EditorGUILayout.IntField("Size", effects.Length);
        if (newSize != effects.Length)
        {
            System.Array.Resize(ref effects, newSize);
        }

        for (int i = 0; i < effects.Length; i++)
        {
            effects[i] = (GameObject)EditorGUILayout.ObjectField("Effect " + (i + 1), effects[i], typeof(GameObject), true);
        }
    }

    private void DisplayEffectManagerArray(ref GameObject[] managers, string prefsKey)
    {
        int newSize = EditorGUILayout.IntField("Size", managers.Length);
        if (newSize != managers.Length)
        {
            System.Array.Resize(ref managers, newSize);
        }

        for (int i = 0; i < managers.Length; i++)
        {
            managers[i] = (GameObject)EditorGUILayout.ObjectField("Manager " + (i + 1), managers[i], typeof(GameObject), true);
        }
    }

    private void DisplayAllTagMaterialDetails()
    {
        if (weatherTextureManager == null)
        {
            return;
        }

        foreach (var tagMaterials in weatherTextureManager.tagMaterialsList)
        {
            if (!materialFoldoutStates.ContainsKey(tagMaterials.tag))
            {
                materialFoldoutStates[tagMaterials.tag] = false;
            }

            materialFoldoutStates[tagMaterials.tag] = EditorGUILayout.Foldout(materialFoldoutStates[tagMaterials.tag], $"Materials for Tag: {tagMaterials.tag}");

            if (materialFoldoutStates[tagMaterials.tag])
            {
                switch (selectedWeatherIndex)
                {
                    case 0:
                        tagMaterials.sunnyMaterial = (Material)EditorGUILayout.ObjectField("Sunny Material", tagMaterials.sunnyMaterial, typeof(Material), false);
                        DisplayMaterialProperties(tagMaterials.sunnyMaterial);
                        break;
                    case 1:
                        tagMaterials.rainyMaterial = (Material)EditorGUILayout.ObjectField("Rainy Material", tagMaterials.rainyMaterial, typeof(Material), false);
                        DisplayMaterialProperties(tagMaterials.rainyMaterial);
                        break;
                    case 2:
                        tagMaterials.snowyMaterial = (Material)EditorGUILayout.ObjectField("Snowy Material", tagMaterials.snowyMaterial, typeof(Material), false);
                        DisplayMaterialProperties(tagMaterials.snowyMaterial);
                        break;
                }
            }
        }
    }

    private void DisplayMaterialProperties(Material material)
    {
        if (material == null)
        {
            return;
        }

        Shader shader = material.shader;

        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
        {
            var propertyType = ShaderUtil.GetPropertyType(shader, i);
            var propertyName = ShaderUtil.GetPropertyName(shader, i);

            if (System.Array.Exists(unwantedProperties, element => propertyName.Contains(element)))
            {
                continue;
            }

            if (propertyType == ShaderUtil.ShaderPropertyType.Color)
            {
                Color color = material.GetColor(propertyName);
                Color newColor = EditorGUILayout.ColorField(propertyName, color);
                if (color != newColor)
                {
                    material.SetColor(propertyName, newColor);
                }
            }
            else if (propertyType == ShaderUtil.ShaderPropertyType.Float || propertyType == ShaderUtil.ShaderPropertyType.Range)
            {
                float value = material.GetFloat(propertyName);
                float newValue = EditorGUILayout.FloatField(propertyName, value);
                if (value != newValue)
                {
                    material.SetFloat(propertyName, newValue);
                }
            }
            else if (propertyType == ShaderUtil.ShaderPropertyType.Vector)
            {
                Vector4 vector = material.GetVector(propertyName);
                Vector4 newVector = EditorGUILayout.Vector4Field(propertyName, vector);
                if (vector != newVector)
                {
                    material.SetVector(propertyName, newVector);
                }
            }
            else if (propertyType == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                Texture texture = material.GetTexture(propertyName);
                Texture newTexture = (Texture)EditorGUILayout.ObjectField(propertyName, texture, typeof(Texture), false);
                if (texture != newTexture)
                {
                    material.SetTexture(propertyName, newTexture);
                }
            }
        }
    }

    private void DisplayEffectDetails()
    {
        GameObject[] selectedEffects = null;
        switch (selectedWeatherIndex)
        {
            case 0:
                selectedEffects = sunnyEffects;
                break;
            case 1:
                selectedEffects = rainyEffects;
                break;
            case 2:
                selectedEffects = snowyEffects;
                break;
        }

        if (selectedEffects != null)
        {
            foreach (var effect in selectedEffects)
            {
                if (effect != null)
                {
                    if (!effectFoldoutStates.ContainsKey(effect))
                    {
                        effectFoldoutStates[effect] = false;
                    }

                    effectFoldoutStates[effect] = EditorGUILayout.Foldout(effectFoldoutStates[effect], effect.name);

                    if (effectFoldoutStates[effect])
                    {
                        var particleSystem = effect.GetComponent<ParticleSystem>();
                        if (particleSystem != null)
                        {
                            var main = particleSystem.main;
                            var emission = particleSystem.emission;

                            if (effect.name.ToLower().Contains("rain"))
                            {
                                // Rain Drop Speed
                                float startSpeed = main.startSpeed.constant;
                                float newStartSpeed = EditorGUILayout.FloatField("Rain Drop Speed", startSpeed);
                                if (newStartSpeed != startSpeed)
                                {
                                    main.startSpeed = newStartSpeed;
                                }

                                // Rain Drop Size
                                float startSize = main.startSize.constant;
                                float newStartSize = EditorGUILayout.FloatField("Rain Drop Size", startSize);
                                if (newStartSize != startSize)
                                {
                                    main.startSize = newStartSize;
                                }

                                // Rain Density (Rate Over Time)
                                float rateOverTime = emission.rateOverTime.constant;
                                float newRateOverTime = EditorGUILayout.FloatField("Rain Density (Rate Over Time)", rateOverTime);
                                if (newRateOverTime != rateOverTime)
                                {
                                    emission.rateOverTime = newRateOverTime;
                                }

                                // Rain Density (Rate Over Distance)
                                float rateOverDistance = emission.rateOverDistance.constant;
                                float newRateOverDistance = EditorGUILayout.FloatField("Rain Density (Rate Over Distance)", rateOverDistance);
                                if (newRateOverDistance != rateOverDistance)
                                {
                                    emission.rateOverDistance = newRateOverDistance;
                                }
                            }

                            if (effect.name.ToLower().Contains("snow"))
                            {
                                // Snow Gravity Modifier
                                float gravityModifier = main.gravityModifier.constant;
                                float newGravityModifier = EditorGUILayout.FloatField("Snow Gravity Modifier", gravityModifier);
                                if (newGravityModifier != gravityModifier)
                                {
                                    main.gravityModifier = newGravityModifier;
                                }

                                // Snow Start Lifetime
                                float startLifetime = main.startLifetime.constant;
                                float newStartLifetime = EditorGUILayout.FloatField("Snow Start Lifetime", startLifetime);
                                if (newStartLifetime != startLifetime)
                                {
                                    main.startLifetime = newStartLifetime;
                                }

                                // Snow Density (Rate Over Time)
                                float rateOverTime = emission.rateOverTime.constant;
                                float newRateOverTime = EditorGUILayout.FloatField("Snow Density (Rate Over Time)", rateOverTime);
                                if (newRateOverTime != rateOverTime)
                                {
                                    emission.rateOverTime = newRateOverTime;
                                }

                                // Snow Density (Rate Over Distance)
                                float rateOverDistance = emission.rateOverDistance.constant;
                                float newRateOverDistance = EditorGUILayout.FloatField("Snow Density (Rate Over Distance)", rateOverDistance);
                                if (newRateOverDistance != rateOverDistance)
                                {
                                    emission.rateOverDistance = newRateOverDistance;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void ApplyWeather(int index)
    {
        // Deactivate all effect managers first
        DeactivateEffectManagers(rainEffectManagerGameObjects);
        DeactivateEffectManagers(snowEffectManagerGameObjects);

        switch (index)
        {
            case 0:
                skyboxRenderer.sharedMaterial = sunnySkyboxMaterial;
                RenderSettings.skybox = sunnySkyboxMaterial;
                if (!disableCurrentWeatherEffects)
                {
                    SetWeatherEffects(sunnyEffects);
                }
                else
                {
                    DisableAllEffects();
                }
                if (weatherTextureManager != null)
                {
                    Debug.Log("Applying sunny materials");
                    weatherTextureManager.ApplySunnyMaterials();
                }
                if (weatherParameterChanger != null)
                {
                    weatherParameterChanger.ApplySunnyProperties(); // Apply sunny material properties
                }
                break;
            case 1:
                skyboxRenderer.sharedMaterial = rainySkyboxMaterial;
                RenderSettings.skybox = rainySkyboxMaterial;
                if (!disableCurrentWeatherEffects)
                {
                    SetWeatherEffects(rainyEffects);
                    ActivateEffectManagers(rainEffectManagerGameObjects);
                }
                else
                {
                    DisableAllEffects();
                }
                if (weatherTextureManager != null)
                {
                    Debug.Log("Applying rainy materials");
                    weatherTextureManager.ApplyRainyMaterials();
                }
                if (weatherParameterChanger != null)
                {
                    weatherParameterChanger.ApplyRainyProperties(); // Apply rainy material properties
                }
                break;
            case 2:
                skyboxRenderer.sharedMaterial = snowySkyboxMaterial;
                RenderSettings.skybox = snowySkyboxMaterial;
                if (!disableCurrentWeatherEffects)
                {
                    SetWeatherEffects(snowyEffects);
                    ActivateEffectManagers(snowEffectManagerGameObjects);
                }
                else
                {
                    DisableAllEffects();
                }
                if (weatherTextureManager != null)
                {
                    Debug.Log("Applying snowy materials");
                    weatherTextureManager.ApplySnowyMaterials();
                }
                if (weatherParameterChanger != null)
                {
                    weatherParameterChanger.ApplySnowyProperties(); // Apply snowy material properties
                }
                break;
        }

        // Ensure Weather Texture Manager is active
        ActivateEffectManagers(weatherTextureManagerGameObjects);

        DynamicGI.UpdateEnvironment();
    }

    private void SetWeatherEffects(GameObject[] effects)
    {
        DisableAllEffects();

        foreach (var effect in effects)
        {
            if (effect != null)
            {
                effect.SetActive(true);
            }
        }
    }

    private void DisableAllEffects()
    {
        foreach (var effect in sunnyEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }

        foreach (var effect in rainyEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }

        foreach (var effect in snowyEffects)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
    }

    private void DeactivateEffectManagers(GameObject[] managers)
    {
        if (managers != null)
        {
            foreach (var manager in managers)
            {
                if (manager != null)
                {
                    manager.SetActive(false);
                }
            }
        }
    }

    private void ActivateEffectManagers(GameObject[] managers)
    {
        if (managers != null)
        {
            foreach (var manager in managers)
            {
                if (manager != null)
                {
                    manager.SetActive(true);
                }
            }
        }
    }

    private void SaveSettings()
    {
        SaveMaterialSettings("SunnySkyboxMaterial", sunnySkyboxMaterial);
        SaveMaterialSettings("RainySkyboxMaterial", rainySkyboxMaterial);
        SaveMaterialSettings("SnowySkyboxMaterial", snowySkyboxMaterial);

        SaveEffectsSettings("SunnyEffects", sunnyEffects);
        SaveEffectsSettings("RainyEffects", rainyEffects);
        SaveEffectsSettings("SnowyEffects", snowyEffects);

        SaveSceneObjectSettings("SkyboxRenderer", skyboxRenderer?.gameObject);
        SaveEffectManagerSettings("RainEffectManagers", rainEffectManagerGameObjects);
        SaveEffectManagerSettings("SnowEffectManagers", snowEffectManagerGameObjects);
        SaveEffectManagerSettings("WeatherTextureManagers", weatherTextureManagerGameObjects);
        SaveSceneObjectSettings("WeatherParameterChanger", weatherParameterChanger?.gameObject);

        EditorPrefs.SetInt(EditorPrefsKey + "SelectedWeatherIndex", selectedWeatherIndex);
        EditorPrefs.SetBool(EditorPrefsKey + "DisableCurrentWeatherEffects", disableCurrentWeatherEffects);
        SaveFoldoutStates();
    }

    private void LoadSettings()
    {
        sunnySkyboxMaterial = LoadMaterialSettings("SunnySkyboxMaterial");
        rainySkyboxMaterial = LoadMaterialSettings("RainySkyboxMaterial");
        snowySkyboxMaterial = LoadMaterialSettings("SnowySkyboxMaterial");

        sunnyEffects = LoadEffectsSettings("SunnyEffects");
        rainyEffects = LoadEffectsSettings("RainyEffects");
        snowyEffects = LoadEffectsSettings("SnowyEffects");

        skyboxRenderer = LoadSceneObjectSettings("SkyboxRenderer")?.GetComponent<Renderer>();
        rainEffectManagerGameObjects = LoadEffectManagerSettings("RainEffectManagers");
        snowEffectManagerGameObjects = LoadEffectManagerSettings("SnowEffectManagers");
        weatherTextureManagerGameObjects = LoadEffectManagerSettings("WeatherTextureManagers");

        if (weatherTextureManagerGameObjects != null && weatherTextureManagerGameObjects.Length > 0)
        {
            weatherTextureManager = weatherTextureManagerGameObjects[0].GetComponent<WeatherTextureManager>();
        }

        weatherParameterChanger = LoadSceneObjectSettings("WeatherParameterChanger")?.GetComponent<WeatherParameterChanger>();

        selectedWeatherIndex = EditorPrefs.GetInt(EditorPrefsKey + "SelectedWeatherIndex", 0);
        disableCurrentWeatherEffects = EditorPrefs.GetBool(EditorPrefsKey + "DisableCurrentWeatherEffects", false);
        LoadFoldoutStates();
    }

    private void SaveMaterialSettings(string key, Material material)
    {
        if (material != null)
        {
            EditorPrefs.SetString(EditorPrefsKey + key, AssetDatabase.GetAssetPath(material));
        }
    }

    private Material LoadMaterialSettings(string key)
    {
        string path = EditorPrefs.GetString(EditorPrefsKey + key, "");
        if (!string.IsNullOrEmpty(path))
        {
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }
        return null;
    }

    private void SaveEffectsSettings(string key, GameObject[] effects)
    {
        EditorPrefs.SetInt(EditorPrefsKey + key + "Length", effects.Length);
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] != null)
            {
                EditorPrefs.SetString(EditorPrefsKey + key + i, GetSceneObjectPath(effects[i]));
            }
        }
    }

    private GameObject[] LoadEffectsSettings(string key)
    {
        int length = EditorPrefs.GetInt(EditorPrefsKey + key + "Length", 0);
        GameObject[] effects = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            string path = EditorPrefs.GetString(EditorPrefsKey + key + i, "");
            if (!string.IsNullOrEmpty(path))
            {
                effects[i] = FindSceneObjectByPath(path);
            }
        }
        return effects;
    }

    private void SaveEffectManagerSettings(string key, GameObject[] managers)
    {
        EditorPrefs.SetInt(EditorPrefsKey + key + "Length", managers.Length);
        for (int i = 0; i < managers.Length; i++)
        {
            if (managers[i] != null)
            {
                EditorPrefs.SetString(EditorPrefsKey + key + i, GetSceneObjectPath(managers[i]));
            }
        }
    }

    private GameObject[] LoadEffectManagerSettings(string key)
    {
        int length = EditorPrefs.GetInt(EditorPrefsKey + key + "Length", 0);
        GameObject[] managers = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            string path = EditorPrefs.GetString(EditorPrefsKey + key + i, "");
            if (!string.IsNullOrEmpty(path))
            {
                managers[i] = FindSceneObjectByPath(path);
            }
        }
        return managers;
    }

    private void SaveSceneObjectSettings(string key, GameObject gameObject)
    {
        if (gameObject != null)
        {
            EditorPrefs.SetString(EditorPrefsKey + key, GetSceneObjectPath(gameObject));
        }
    }

    private GameObject LoadSceneObjectSettings(string key)
    {
        string path = EditorPrefs.GetString(EditorPrefsKey + key, "");
        if (!string.IsNullOrEmpty(path))
        {
            return FindSceneObjectByPath(path);
        }
        return null;
    }

    private string GetSceneObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private GameObject FindSceneObjectByPath(string path)
    {
        string[] splitPath = path.Split('/');
        GameObject obj = null;
        foreach (string name in splitPath)
        {
            if (obj == null)
            {
                obj = GameObject.Find(name);
            }
            else
            {
                Transform childTransform = obj.transform.Find(name);
                if (childTransform != null)
                {
                    obj = childTransform.gameObject;
                }
                else
                {
                    return null;
                }
            }
        }
        return obj;
    }

    private GameObject FindInactiveObjectByPath(string path)
    {
        string[] splitPath = path.Split('/');
        GameObject obj = null;
        foreach (string name in splitPath)
        {
            if (obj == null)
            {
                obj = FindInActiveObjectByName(name);
            }
            else
            {
                Transform childTransform = obj.transform.Find(name);
                if (childTransform != null)
                {
                    obj = childTransform.gameObject;
                }
                else
                {
                    return null;
                }
            }
        }
        return obj;
    }

    private GameObject FindInActiveObjectByName(string name)
    {
        GameObject[] objs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in objs)
        {
            if (obj.name == name)
            {
                return obj;
            }
        }
        return null;
    }

    private void SaveFoldoutStates()
    {
        foreach (var kvp in materialFoldoutStates)
        {
            EditorPrefs.SetBool(EditorPrefsKey + "Foldout_" + kvp.Key, kvp.Value);
        }

        foreach (var kvp in effectFoldoutStates)
        {
            EditorPrefs.SetBool(EditorPrefsKey + "EffectFoldout_" + kvp.Key.GetInstanceID(), kvp.Value);
        }
    }

    private void LoadFoldoutStates()
    {
        if (weatherTextureManager == null)
        {
            return;
        }

        foreach (var tagMaterials in weatherTextureManager.tagMaterialsList)
        {
            if (EditorPrefs.HasKey(EditorPrefsKey + "Foldout_" + tagMaterials.tag))
            {
                materialFoldoutStates[tagMaterials.tag] = EditorPrefs.GetBool(EditorPrefsKey + "Foldout_" + tagMaterials.tag);
            }
            else
            {
                materialFoldoutStates[tagMaterials.tag] = false;
            }
        }

        GameObject[] allEffects = GetAllEffects();
        foreach (var effect in allEffects)
        {
            if (EditorPrefs.HasKey(EditorPrefsKey + "EffectFoldout_" + effect.GetInstanceID()))
            {
                effectFoldoutStates[effect] = EditorPrefs.GetBool(EditorPrefsKey + "EffectFoldout_" + effect.GetInstanceID());
            }
            else
            {
                effectFoldoutStates[effect] = false;
            }
        }
    }

    private GameObject[] GetAllEffects()
    {
        List<GameObject> allEffects = new List<GameObject>();
        allEffects.AddRange(sunnyEffects);
        allEffects.AddRange(rainyEffects);
        allEffects.AddRange(snowyEffects);
        return allEffects.ToArray();
    }
}
