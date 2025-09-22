using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.WeatherPreset
{
    [System.Serializable]
    public class WeatherPresetConfig 
    {
        [BoxGroup("Settings"),
         Required,
         ShowInInspector] public List<WeatherPreset> clearWeatherPreset;
        
        [BoxGroup("Settings"),
         Required,
         ShowInInspector] public List<WeatherPreset> rainyWeatherPreset;
        
        [BoxGroup("Settings"),
         Required,
         ShowInInspector] public List<WeatherPreset> fogWeatherPreset;
    }
}
