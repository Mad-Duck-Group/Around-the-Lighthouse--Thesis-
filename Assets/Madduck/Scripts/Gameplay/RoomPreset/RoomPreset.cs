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
         SerializeField] public SpriteRenderer[] rockRenderers;
        [BoxGroup("References"),
         SerializeField] public SpriteRenderer[] waveRenderers;
    
        [Title("Variants")]
        [BoxGroup("Variants"),
         SerializeField] private Sprite[] rockVariants;
        [BoxGroup("Variants"),
         SerializeField] private Sprite[] waveVariants;
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
        SerializeField] private TweenAnimWaveSpeed _waveDurationMultiplier ;

    
        private Sequence _waveSequence;
        private DayPhaseType _currentDayPhase;
        private ReactiveProperty<WeatherType> _currentWeather { get; set; } = new();

        #endregion

        #region Set Up Room

        public void SetDynamicElements(ReactiveProperty<WeatherType> weatherType)
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
        
            if (rockRenderers != null && rockVariants.Length > 0)
            {
                foreach (var render in rockRenderers)
                {
                    if (render)
                        render.sprite = rockVariants.GetRandomElement();
                }
            }
        
            if (waveRenderers != null && waveVariants.Length > 0)
            {
                foreach (var render in waveRenderers)
                {
                    if (render)
                        render.sprite = waveVariants.GetRandomElement();
                }
            }
            AnimateWave();
            //ShakeRock();
        }   
        #endregion

        #region Tween
        public void AnimateWave()
        {
            if (waveRenderers == null || waveRenderers.Length == 0) return;
            SetSpeedTween();
            foreach (var waveRenderer in waveRenderers)
            {
                if (!waveRenderer) continue;
                var wave = waveRenderer.transform;
                float startY = wave.localPosition.y; 
                var relativeSettings = waveTweenSettings.ToRelative(startY);
                var startDelay = Random.Range(0f, waveTweenSettings.settings.duration);
                var cycle = relativeSettings.settings.cycles;
                relativeSettings.settings.cycles = 1;
                relativeSettings.settings.startDelay = startDelay;
                _waveSequence = Sequence.Create(cycle, CycleMode.Yoyo)
                    .Group(Tween.LocalPositionY(wave, relativeSettings));
            }
        }

        public void SetSpeedTween()
        {
            var waveDuration = 0f;
            switch (_currentWeather.Value)
            {
                case WeatherType.Clear:
                    waveDuration = _waveDurationMultiplier.WeatherTypeClearSpeed;
                    break;
                case WeatherType.Rain:
                    waveDuration = _waveDurationMultiplier.WeatherTypeRainSpeed;
                    break;
                case WeatherType.Storm:
                    waveDuration = _waveDurationMultiplier.WeatherTypeStormSpeed;
                    break;
                case WeatherType.StrongWinds:
                    waveDuration = _waveDurationMultiplier.WeatherTypeStrongWindsSpeed;
                    break;
                case WeatherType.Cloudy:
                    waveDuration = _waveDurationMultiplier.WeatherTypeCloudySpeed;
                    break;
                default:
                    waveDuration = _waveDurationMultiplier.WeatherTypeClearSpeed;
                    break;  
            }
            waveTweenSettings.settings.duration *=  waveDuration;
            
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

        public void StopWave()
        {
            _waveSequence.Complete();
        }
        #endregion

    }
}
