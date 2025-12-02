using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Shared;
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
         SerializeField] private bool isStormy;

        [BoxGroup("Settings"),
         ShowIf(nameof(isStormy)),
         SerializeField] private TweenSettings<Color> stormTweenSettings;


        [BoxGroup("Settings"),
         ShowIf(nameof(isStormy)),
         MinMaxSlider(nameof(_dynamicRange), true),
         SerializeField] private Vector2 stormDelayMinMax = new(1, 10);

        [Required,
         SerializeField] private Light2D globalLight;

        #endregion

        #region Fields

        private readonly Vector2 _dynamicRange = new(0, 10);

        #endregion

        #region Set Up Weather Particles

        public void SetUpWeatherParticles()
        {
            if (particleEntries == null) return;
            foreach (var particle in particleEntries)
            {
                if (!particle.renderer || particle.config == null) continue;
                particle.prefab.Play();
                particle.config.ApplyTo(particle.renderer);
            }

            if (isStormy)
            {
                StormAnim();
            }
        }

        #endregion

        #region Storm Light Animation

        private void StormAnim()
        {
            Sequence.Create()
                .Chain(Tween.Custom(stormTweenSettings.startValue, stormTweenSettings.endValue,
                    stormTweenSettings.settings,
                    x => globalLight.color = x))
                .Chain(Tween.Custom(stormTweenSettings.endValue, stormTweenSettings.startValue,
                    stormTweenSettings.settings,
                    x => globalLight.color = x))
                .Chain(Tween.Custom(stormTweenSettings.startValue, stormTweenSettings.endValue,
                    stormTweenSettings.settings,
                    x => globalLight.color = x))
                .Chain(Tween.Custom(stormTweenSettings.endValue, stormTweenSettings.startValue,
                    stormTweenSettings.settings,
                    x => globalLight.color = x))
                .ChainDelay(UnityEngine.Random.Range(_dynamicRange.x, _dynamicRange.y))
                .SetRemainingCycles(-1);
        }

        #endregion
    }
}