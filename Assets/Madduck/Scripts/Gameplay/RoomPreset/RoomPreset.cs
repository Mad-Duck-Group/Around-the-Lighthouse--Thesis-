using Madduck.Shared;
using Madduck.Utils;
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
        SerializeField] private float _waveSpeedMultiplier = 1f;

    
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
                var startDelay = Random.Range(0f, waveTweenSettings.settings.startDelay);
                var cycle = relativeSettings.settings.cycles;
                relativeSettings.settings.cycles = 1;
                relativeSettings.settings.startDelay = startDelay;
                _waveSequence = Sequence.Create(cycle, CycleMode.Yoyo)
                    .Group(Tween.LocalPositionY(wave, relativeSettings));
            }
        }

        public void SetSpeedTween()
        {
            switch (_currentWeather.Value)
            {
                case WeatherType.Clear:
                    _waveSpeedMultiplier = 1f;
                    break;
                case WeatherType.Rain:
                    _waveSpeedMultiplier = 0.5f;
                    break;
                case WeatherType.Storm:
                    _waveSpeedMultiplier = 0.25f;
                    break;
                case WeatherType.StrongWinds:
                    _waveSpeedMultiplier = 0.75f;
                    break;
                case WeatherType.Cloudy:
                    _waveSpeedMultiplier = 0.9f;
                    break;
                default:
                    _waveSpeedMultiplier = 1f;
                    break;  
            }
            waveTweenSettings.settings.duration =  _waveSpeedMultiplier;
            
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
