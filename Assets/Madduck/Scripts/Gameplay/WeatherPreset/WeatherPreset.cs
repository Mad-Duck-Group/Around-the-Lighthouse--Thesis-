using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.WeatherPreset
{
  public class WeatherPreset : MonoBehaviour
  {
   
     #region Inspector

     [BoxGroup("References"),
      SerializeField]
     private Transform particleParent;
      
     [BoxGroup("References"),
      SerializeField]
     private Light[] lightnings;
      
     #endregion

     #region Settings

     [BoxGroup("Settings"),
     SerializeField] private ParticleEntry[] particleEntries;

     #endregion

     #region Set Up Weather Particles

     public void SetUpWeatherParticles()
     {
         if (particleEntries == null) return;
         foreach (var particle in particleEntries)
         {
             if (particle.renderer != null && particle.config != null)
             {
                 particle.config.ApplyTo(particle.renderer);
             }
         }
     }
      
     
     #endregion
   }
}

