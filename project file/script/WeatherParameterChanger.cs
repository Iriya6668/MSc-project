using UnityEngine;

public class WeatherParameterChanger : MonoBehaviour
{
    [System.Serializable]
    public class MaterialSettings
    {
        
        public Material material;
        // snow amount in different weather
        public float sunnySnowAmount;
        public float rainySnowAmount;
        public float snowySnowAmount;
    }

    public MaterialSettings[] materialSettings;

    public void ApplySunnyProperties()
    {
        ApplyProperties(WeatherType.Sunny);
    }

    public void ApplyRainyProperties()
    {
        ApplyProperties(WeatherType.Rainy);
    }

    public void ApplySnowyProperties()
    {
        ApplyProperties(WeatherType.Snowy);
    }

    private void ApplyProperties(WeatherType weatherType)
    {
        foreach (var setting in materialSettings)
        {
            if (setting.material == null) continue;

            float snowAmount = 0;
            switch (weatherType)
            {
                case WeatherType.Sunny:
                    snowAmount = setting.sunnySnowAmount;
                    break;
                case WeatherType.Rainy:
                    snowAmount = setting.rainySnowAmount;
                    break;
                case WeatherType.Snowy:
                    snowAmount = setting.snowySnowAmount;
                    break;
            }

            setting.material.SetFloat("_SnowAmount", snowAmount);
        }
    }

    public enum WeatherType
    {
        Sunny,
        Rainy,
        Snowy
    }
}
