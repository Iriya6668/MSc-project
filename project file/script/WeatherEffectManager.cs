using UnityEngine;
using System.Collections.Generic;

public class WeatherEffectManager : MonoBehaviour
{
    public ParticleSystem rainEffectPrefab; // Rain particle system
    public Transform vehicleTransform; // Vehicle's transform
    public Transform roadTransform; // Road's transform
    public float roadLength = 270f; // Length of each road segment
    public float roadWidth = 60f; // Width of each road segment
    public float activationRadius = 150f; // Activation radius
    public float deactivationRadius = 180f; // Deactivation radius
    public int preloadZoneCount = 3; // Preload zone count
    public int direction = 1; // Generation direction, 1 for forward, -1 for backward
    public float customYOffset = 0f; // Custom Y offset, 0 to use default Y value

    private List<ParticleSystem> rainEffectZones = new List<ParticleSystem>(); // List of rain effect zones
    private int initialRoadIndex = 0; // Initial road segment index
    private int currentRoadIndex = 0; // Current road index

    private bool isActive = true; // Script active state


    void Start()
    {
        if (vehicleTransform == null || rainEffectPrefab == null || roadTransform == null)
        {
            Debug.LogWarning("Vehicle Transform, Rain Effect Prefab, or Road Transform is not assigned.");
            return;
        }

        currentRoadIndex = initialRoadIndex;

        // Preload zones
        for (int i = 0; i < preloadZoneCount; i++)
        {
            GenerateRainEffectZone(currentRoadIndex + i * direction);
        }

        UpdateRainEffectZones();
    }

    void Update()
    {
        if (!isActive) return;

        if (vehicleTransform == null)
        {
            Debug.LogWarning("Vehicle Transform is not assigned.");
            return;
        }

        int newRoadIndex = Mathf.FloorToInt((vehicleTransform.position.z - roadTransform.position.z) / roadLength) * direction;
        if (newRoadIndex != currentRoadIndex)
        {
            currentRoadIndex = newRoadIndex;

            GenerateRainEffectZone(currentRoadIndex + direction);
        }

        UpdateRainEffectZones();
    }

    void GenerateRainEffectZone(int roadIndex)
    {
        // caculate the next effect area position
        Vector3 roadCenter = roadTransform.position + roadTransform.forward * (roadIndex * roadLength + roadLength / 2) * direction;
        Quaternion roadRotation = roadTransform.rotation;

        // Apply custom Y offset
        if (customYOffset != 0f)
        {
            roadCenter.y = customYOffset;
        }

        if (!rainEffectZones.Exists(zone => Vector3.Distance(zone.transform.position, roadCenter) < roadLength / 2))
        {
            ParticleSystem rainEffect = Instantiate(rainEffectPrefab, roadCenter, roadRotation);
            rainEffect.transform.SetParent(transform); // Set the particle system as a child of this object
            rainEffect.gameObject.SetActive(false); // Initially inactive
            rainEffectZones.Add(rainEffect);
        }
    }

    void UpdateRainEffectZones()
    {
        foreach (ParticleSystem rainEffect in rainEffectZones)
        {
            float distanceToVehicle = Vector3.Distance(vehicleTransform.position, rainEffect.transform.position);
            if (distanceToVehicle <= activationRadius)
            {
                if (!rainEffect.gameObject.activeSelf)
                {
                    rainEffect.gameObject.SetActive(true);
                }
                if (!rainEffect.isPlaying)
                {
                    rainEffect.Play();
                }
            }
            else if (distanceToVehicle >= deactivationRadius)
            {
                if (rainEffect.isPlaying)
                {
                    rainEffect.Stop();
                }
                if (rainEffect.gameObject.activeSelf)
                {
                    rainEffect.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ClearAllRainEffectZones()
    {
        foreach (ParticleSystem rainEffect in rainEffectZones)
        {
            Destroy(rainEffect.gameObject);
        }
        rainEffectZones.Clear();
    }

    public void DeactivateAllRainEffectZones()
    {
        foreach (ParticleSystem rainEffect in rainEffectZones)
        {
            if (rainEffect.isPlaying)
            {
                rainEffect.Stop();
            }
            rainEffect.gameObject.SetActive(false);
        }
    }

    public void ActivateAllRainEffectZones()
    {
        foreach (ParticleSystem rainEffect in rainEffectZones)
        {
            if (!rainEffect.gameObject.activeSelf)
            {
                rainEffect.gameObject.SetActive(true);
            }
            if (!rainEffect.isPlaying)
            {
                rainEffect.Play();
            }
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive)
        {
            DeactivateAllRainEffectZones();
        }
        else
        {
            ActivateAllRainEffectZones();
        }
    }
}
