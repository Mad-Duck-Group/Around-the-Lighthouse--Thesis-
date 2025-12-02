using System.Collections.Generic;
using Madduck.GameData;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.WeatherPreset
{
    [System.Serializable]
    public class WeatherParticlesSettings
    {
        
        public WindDirection WindDirections;
        public ParticleSystem[] ParticleSystem;
        public Vector2 PositionOffset;
        
    }
    [System.Serializable]
    public class WeatherParticleGroup
    {
        [TableList]
        public List<WeatherParticlesSettings> ParticlesSettings;
        public bool isStormy;
        [ShowIf(nameof(isStormy))]
        public ParticleSystem StormParticleSystem;
        [ShowIf(nameof(isStormy))]
        public List<WeatherParticlesSettings> windsStormyParticle;
        [ShowIf(nameof(isStormy))]
        public Vector2 StormPositionOffset;
        public bool isRainy;
        [ShowIf(nameof(isRainy))]
        public List<WeatherParticlesSettings> windsRainyParticle;
    }
    [System.Serializable]
    public class WeatherPresetConfig 
    {
     
        [BoxGroup("Settings"),
         Required,
         ShowInInspector] public SerializableDictionary<WeatherType,WeatherPreset> weatherPreset;
        [BoxGroup("Settings"),
         Required,
         ShowInInspector]public SerializableDictionary<WeatherType,WeatherParticleGroup> weatherParticles;
        
    }
}
