using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using Madduck.WeatherPreset;
using PrimeTween;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.RoomPreset
{
    public class RoomPreset : MonoBehaviour
    {
        #region Inspector

        [Title("References"),
         BoxGroup("References"),
         SerializeField] public SpriteRenderer skyRenderer;
        [BoxGroup("References"),
         SerializeField] public SpriteRenderer[] waveRenderers;
        [BoxGroup("References"),
         SerializeField] public EnvironmentAnim[] environmentAnims;
        
    
        [Title("Variants")]
        
        [BoxGroup("Variants"),
         SerializeField] private Sprite[] waveVariants;
        [BoxGroup("Variants"),
         SerializeField] private Sprite[] waveRainVariants;
        [BoxGroup("Variants"),
         SerializeField] private Sprite[] daySkyVariants;
        [BoxGroup("Variants"),
         SerializeField] private Sprite[] nightSkyVariants;
    
        [Title("Tween Settings"),
         BoxGroup("Tween Settings"),
         SerializeField] public ShakeSettings rockShakeSettings;
        [BoxGroup("Tween Settings"),
         SerializeField] public TweenSettings<float> waveTweenSettings;
        [BoxGroup("Tween Settings"),
        SerializeField] private SerializableDictionary<WeatherType,float> waveDurationMultiplier ;
        
        private DayPhaseType _currentDayPhase;
        private WeatherType _currentWeather;

        #endregion

        #region Set Up Room

        public void SetDynamicElements(WeatherType weatherType)
        {
            _currentWeather = weatherType;
        }

        public void SetDayPhase(DayPhaseType dayPhase)
        {
            _currentDayPhase = dayPhase;
        }

        public void ApplySprites()
        {
            if (skyRenderer)
            {
                switch (_currentDayPhase)
                {
                    case DayPhaseType.Day when daySkyVariants.Length > 0:
                        skyRenderer.sprite = daySkyVariants.GetRandomElement();
                        break;
                    case DayPhaseType.Night when nightSkyVariants.Length > 0:
                        skyRenderer.sprite = nightSkyVariants.GetRandomElement();
                        break;
                }
            }
        
            if (waveRenderers != null && waveVariants.Length > 0)
            {
                for (int i = 0; i < waveRenderers.Length; i++)
                {
                    if (_currentWeather is WeatherType.Rain or WeatherType.Storm )
                    {
                        waveRenderers[i].sprite = waveRainVariants[i];
                    }
                    else
                    {
                        waveRenderers[i].sprite = waveVariants[i];

                    }
                }
            }
            AnimateWaves();
            
        }

        public void ApplyAnimation()
        {
            foreach (var environment in environmentAnims)
            {
                environment.SetAnimator(_currentWeather);
            }
        }
        #endregion

        #region Tween
        private void AnimateWaves()
        {
            if (waveRenderers == null || waveRenderers.Length == 0) return;
            SetSpeedTween();
            foreach (var waveRenderer in waveRenderers)
            {
                if (!waveRenderer) continue;
                var wave = waveRenderer.transform;
                AnimateWave(wave).Forget();
            }
        }

        private async UniTaskVoid AnimateWave(Transform wave)
        {
            var startY = wave.localPosition.y; 
            var relativeSettings = waveTweenSettings.ToRelative(startY);
            var startDelay = Random.Range(0f, waveTweenSettings.settings.startDelay);
            relativeSettings.settings.cycles = 1;
            relativeSettings.settings.startDelay = 0f; 
            await UniTask.WaitForSeconds(startDelay);
            var waveSequence = Sequence.Create(-1, CycleMode.Yoyo)
                .Group(Tween.LocalPositionY(wave, relativeSettings));
        }

        private void SetSpeedTween()
        {
            float waveDuration;
            if (waveDurationMultiplier.TryGetValue(_currentWeather, out var resultDuration)){ waveDuration = resultDuration;}
            else
            {
                waveDurationMultiplier.TryGetValue(WeatherType.Clear, out var defaultDuration);
                waveDuration = defaultDuration;
            }
            waveTweenSettings.settings.duration *= waveDuration;
            
        }
        // public void ShakeRock(int index = -1)
        // {
        //         if (rockRenderers == null || rockRenderers.Length == 0) return;
        //
        //         if (index >= 0 && index < rockRenderers.Length)
        //         {
        //            
        //             Tween.ShakeLocalPosition(rockRenderers[0].transform ,rockShakeSettings);
        //         }
        //         else
        //         {
        //             
        //             foreach (var rock in rockRenderers)
        //             {
        //                 if (rock != null)
        //                     Tween.ShakeLocalPosition(rock.transform ,rockShakeSettings);
        //                     
        //             }
        //         }
        // }
        #endregion

    }
}
