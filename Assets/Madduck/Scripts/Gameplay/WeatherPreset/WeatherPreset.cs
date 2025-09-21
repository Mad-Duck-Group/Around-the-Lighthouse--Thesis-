using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Madduck.WeatherPreset
{
  public class WeatherPreset : MonoBehaviour
  {
      
     #region Settings

     [BoxGroup("Settings"),
     SerializeField] private ParticleEntry[] particleEntries;
     
     [BoxGroup("Settings"),
     SerializeField] private bool isStormy = false;
     [BoxGroup("Settings"),
      ShowIf("isStormy"),
     SerializeField]private TweenSettings<Color> stormTweenSettings;
     
     
     [MinMaxSlider("_dynamicRange", true),
      ShowIf("isStormy"),
     BoxGroup("Settings"),
     SerializeField]private Vector2 stormDelayMinMax = new Vector2(1, 10);
     
     [Required,
     SerializeField]private Light2D globalLight;
     
      

     #endregion

     #region Fields
    
     private readonly Vector2 _dynamicRange = new Vector2(0, 10);

     #endregion
       
     #region Set Up Weather Particles

     public void SetUpWeatherParticles()
     {
         if (particleEntries == null) return;
         foreach (var particle in particleEntries)
         {
             if (particle.renderer != null && particle.config != null)
             {
                 particle.prefab.Play();
                 particle.config.ApplyTo(particle.renderer);
             }
         }
         if (isStormy)
         {
             StormAnim();
         }
     }

     
     #endregion

     #region Strome Light Animation
        
     private void StormAnim()
     {
         Light2D light = globalLight;
         Sequence.Create().Chain(Tween.Custom(stormTweenSettings.startValue, stormTweenSettings.endValue, stormTweenSettings.settings,
             x => light.color = x))
             .Chain(Tween.Custom(stormTweenSettings.endValue, stormTweenSettings.startValue, stormTweenSettings.settings,
                 x => light.color = x))
             .Chain(Tween.Custom(stormTweenSettings.startValue, stormTweenSettings.endValue, stormTweenSettings.settings,
                 x => light.color = x))
             .Chain(Tween.Custom(stormTweenSettings.endValue, stormTweenSettings.startValue, stormTweenSettings.settings,
                 x => light.color = x)).ChainDelay(UnityEngine.Random.Range(_dynamicRange.x,_dynamicRange.y)).SetRemainingCycles(-1);
         
         
     }

     #endregion
     
   }
}

