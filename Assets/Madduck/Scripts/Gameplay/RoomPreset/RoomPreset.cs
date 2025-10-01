using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
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
    
        private Sequence _waveSequence;
        private DayPhaseType _currentDayPhase;
        #endregion

        #region Set Up Room

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
