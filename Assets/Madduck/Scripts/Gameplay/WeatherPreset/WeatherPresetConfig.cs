using System.Collections.Generic;
using Madduck.GameData;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.WeatherPreset
{
    public class WeatherParticlesSettings
    {
        public WindDirection WindDirections;
        public ParticleSystem[] ParticleSystem;
        public Vector2 PositionOffset;
        
    }
    [System.Serializable]
    public class WeatherPresetConfig 
    {
     
        [BoxGroup("Settings"),
         Required,
         ShowInInspector] public SerializableDictionary<WeatherType,WeatherPreset> weatherPreset;
        [BoxGroup("Settings"),
         Required,
         ShowInInspector]public SerializableDictionary<WeatherType,List<WeatherParticlesSettings>> weatherParticles;
        
    }
}
