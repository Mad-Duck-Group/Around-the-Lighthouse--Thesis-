using System;
using System.Collections.Generic;
using System.Linq;
using MadDuck.Scripts.Utils.Inspectors;
using PrimeTween;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Scripts.FishingBoard.UI
{
    [Serializable]
    public class FishingBoardMinigameReference
    {
        #region Inspector
        [SerializeField, Required] private Slider fatigueSlider;
        [SerializeField, Required] private Image fishFatigueImage;
        [SerializeField, OnValueChanged(nameof(OnFatigueImageDictionaryChanged))] 
        private SerializableDictionary<Sprite, PercentageMultiplier> fatigueImageDictionary = new();
        [SerializeField, Required] private Slider reelingSlider;
        [FoldoutGroup("Tween")]
        //[SerializeField] private ShakeSettings fatigueSliderShakeSettings;
        [FoldoutGroup("Tween")]
        [SerializeField] private ShakeSettings reelingSliderShakeSettings;
        #endregion
        
        //private Tween _fatigueSliderShakeTween;
        private Tween _reelingSliderShakeTween;
        private List<KeyValuePair<Sprite, PercentageMultiplier>> _sortedFatigueImageList = new();

        private void OnFatigueImageDictionaryChanged()
        {
            //sort the dictionary by value descending
            var sortedDictionary = fatigueImageDictionary.OrderByDescending(pair => pair.Value.percentage).ToList();
            _sortedFatigueImageList = sortedDictionary;
        }
        
        public void Initialize()
        {
            fatigueSlider.minValue = 0;
            fatigueSlider.maxValue = 1;
            fatigueSlider.value = 0;
            reelingSlider.minValue = 0;
            reelingSlider.maxValue = 100;
            reelingSlider.value = 0;
            OnFatigueImageDictionaryChanged();
        }

        public void SetFatigue(float percent)
        {
            fatigueSlider.value = percent;
            foreach (var pair in _sortedFatigueImageList)
            {
                if (percent < pair.Value.percentage) continue;
                fishFatigueImage.sprite = pair.Key;
                break;
            }
        }

        public void ShakeReelingSlider(float percent)
        {
            if (_reelingSliderShakeTween.isAlive) _reelingSliderShakeTween.Complete();
            var copy = reelingSliderShakeSettings;
            copy.strength = reelingSliderShakeSettings.strength * percent;
            copy.frequency = reelingSliderShakeSettings.frequency * percent;
            if (copy.strength.magnitude <= 0f) return;
            _reelingSliderShakeTween = Tween.ShakeLocalPosition(reelingSlider.transform, copy);
        }
    }
}