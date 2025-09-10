using System;
using System.Collections.Generic;
using System.Linq;
using MadDuck.Scripts.Utils.Inspectors;
using PrimeTween;
using R3;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.FishingBoard
{
    public enum FishZone
    {
        Green,
        Yellow,
        Red
    }
    public class FishingBoardView : MonoBehaviour
    {
        [Title("References")] 
        [Required]
        [SerializeField] private CanvasGroup canvasGroup;
        [Required]
        [SerializeField] private RectTransform hookObject;
        [Required]
        [SerializeField] private RectTransform hookIcon;
        [Required]
        [SerializeField] private RectTransform fishObject;
        [Required]
        [SerializeField] private RectTransform fishIcon;
        [Required]
        [SerializeField] private FishingLineHandler fishingLineHandler;
        [Required]
        [SerializeField] private SerializableDictionary<FishZone, CircleBoard> circleBoards = new();
        [Required]
        [SerializeField] private Slider fatigueSlider;
        [Required]
        [SerializeField] private Image fishFatigueImage;
        [Required]
        [SerializeField] private SerializableDictionary<Sprite, PercentageMultiplier> fatigueImageDictionary = new();
        [Required]
        [SerializeField] private Slider reelingSlider;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> fishingBoardScaleTweenSettings;
        [SerializeField] private TweenSettings<float> fishingBoardAlphaTweenSettings;
        [SerializeField] private ShakeSettings shakeTweenSettings;
        [SerializeField] private ShakeSettings reelingSliderShakeSettings;
        
        private Tween _reelingSliderShakeTween;
        private List<KeyValuePair<Sprite, PercentageMultiplier>> _sortedFatigueImageList = new();
        private FishingBoardViewModel _fishingBoardViewModel;
        private IDisposable _isActiveBinding;
        private IDisposable _bindings;
        private Sequence _fishingBoardActivationSequence;
        private Tween _hookShakeTween;
        private Tween _fishShakeTween;

        #region Bindings
        [Inject]
        public void SetUp(FishingBoardViewModel fishingBoardViewModel)
        {
            _fishingBoardViewModel = fishingBoardViewModel;
            _isActiveBinding = 
                _fishingBoardViewModel.IsActive
                    .Subscribe(SetActive);
            UpdateCircleBoardStates();
            Bind();
        }
        
        private void Bind()
        {
            _bindings?.Dispose();
            var disposableBuilder = Disposable.CreateBuilder();
            _fishingBoardViewModel.FishPosition
                .Subscribe(x =>
                {
                    fishObject.localPosition = x;
                    DrawFishLine();
                })
                .AddTo(ref disposableBuilder);
            _fishingBoardViewModel.FishRotation
                .Subscribe(x => fishObject.localRotation = x)
                .AddTo(ref disposableBuilder);
            _fishingBoardViewModel.HookPosition
                .Subscribe(x =>
                {
                    hookObject.localPosition = x;
                    DrawFishLine();
                })
                .AddTo(ref disposableBuilder);
            _fishingBoardViewModel.HookRotation
                .Subscribe(x => hookObject.localRotation = x)
                .AddTo(ref disposableBuilder);
            _fishingBoardViewModel.FishLineDurabilityPercent
                .Subscribe(x =>
                {
                    ShakeHook(x);
                    ShakeFish(x);
                    ShakeReelingSlider(x);
                    fishingLineHandler.HandleTension(x);
                })
                .AddTo(ref disposableBuilder);
            _fishingBoardViewModel.FatigueLevelPercent
                .Subscribe(SetFatigue)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        #endregion

        #region Lifecycle
        private void Start()
        {
            InitializeFishingBoard();
            UpdateCircleBoardStates();
            DrawFishLine();
        }
        
        private void OnDestroy()
        {
            _isActiveBinding?.Dispose();
            _bindings?.Dispose();
            _fishingBoardViewModel.Dispose();
        }
        #endregion
        
        #region Initialization
        /// <summary>
        /// Initialize the fishing board UI elements.
        /// </summary>
        private void InitializeFishingBoard()
        {
            fatigueSlider.minValue = 0;
            fatigueSlider.maxValue = 1;
            fatigueSlider.value = 0;
            reelingSlider.minValue = 0;
            reelingSlider.maxValue = 1;
            reelingSlider.value = 0;
            var sortedDictionary = fatigueImageDictionary.OrderByDescending(pair => pair.Value.percentage).ToList();
            _sortedFatigueImageList = sortedDictionary;
            foreach (var board in circleBoards)
            {
                var rectTransform = board.Value.Circle;
                rectTransform.sizeDelta = new Vector2(board.Value.Radius * 2 , board.Value.Radius * 2 );
            }
            hookObject.localPosition = circleBoards[FishZone.Red].Circle.localPosition;
        }
        
        /// <summary>
        /// Update the states of all circle boards and notify the ViewModel.
        /// </summary>
        private void UpdateCircleBoardStates()
        {
            var circleBoardStates = new Dictionary<FishZone, CircleBoardState>();
            foreach (var board in circleBoards)
            {
                var state = new CircleBoardState(board.Value);
                circleBoardStates.Add(board.Key, state);
            }
            _fishingBoardViewModel.OnCircleBoardUpdated.Execute(circleBoardStates);
        }
        #endregion
        
        #region Activation
        /// <summary>
        /// Set the active state of the fishing board UI.
        /// </summary>
        /// <param name="active"></param>
        private void SetActive(bool active)
        {
            if (active)
            {
                Bind();
                canvasGroup.gameObject.SetActive(true);
            }
            else
            {
                fishingLineHandler.Reset();
                _bindings?.Dispose();
            }
            Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
            if (_fishingBoardActivationSequence.isAlive) _fishingBoardActivationSequence.Complete();
            _fishingBoardActivationSequence = Sequence.Create()
                .Group(Tween.Scale(canvasGroup.transform, fishingBoardScaleTweenSettings.WithDirection(active)))
                .Group(Tween.Alpha(canvasGroup, fishingBoardAlphaTweenSettings.WithDirection(active)))
                .OnComplete(() =>
                { 
                    if (!active) canvasGroup.gameObject.SetActive(false);
                });
        }
        #endregion
        
        #region UI
        /// <summary>
        /// Draw the fishing line.
        /// </summary>
        private void DrawFishLine()
        {
            var center = circleBoards[FishZone.Red].Circle;
            fishingLineHandler.GetWidthHeight( circleBoards[FishZone.Red].Radius * 2, circleBoards[FishZone.Red].Radius * 2);
            fishingLineHandler.SetPoints(hookObject.transform, center.transform, fishObject.transform);
        }
        
        /// <summary>
        /// Shake the hook icon based on fishing line durability.
        /// </summary>
        /// <param name="durabilityPercent"></param>
        private void ShakeHook(float durabilityPercent)
        {
            if (_hookShakeTween.isAlive) return;
            var copy = shakeTweenSettings;
            copy.strength = shakeTweenSettings.strength * (1 - durabilityPercent);
            copy.frequency = shakeTweenSettings.frequency * (1 - durabilityPercent);
            if (copy.strength.magnitude <= 0) return;
            _hookShakeTween = Tween.ShakeLocalPosition(hookIcon.transform, copy);
        }
        
        /// <summary>
        /// Shake the fish icon based on fishing line durability.
        /// </summary>
        /// <param name="durabilityPercent"></param>
        private void ShakeFish(float durabilityPercent)
        {
            if (_fishShakeTween.isAlive) return;
            var copy = shakeTweenSettings;
            copy.strength = shakeTweenSettings.strength * (1 - durabilityPercent);
            copy.frequency = shakeTweenSettings.frequency * (1 - durabilityPercent);
            if (copy.strength.magnitude <= 0) return;
            _fishShakeTween = Tween.ShakeLocalPosition(fishIcon.transform, copy);
        }
        
        /// <summary>
        /// Set the fatigue level UI.
        /// </summary>
        /// <param name="fatiguePercent"></param>
        private void SetFatigue(float fatiguePercent)
        {
            fatigueSlider.value = fatiguePercent;
            foreach (var pair in _sortedFatigueImageList)
            {
                if (fatiguePercent < pair.Value.percentage) continue;
                fishFatigueImage.sprite = pair.Key;
                break;
            }
        }

        /// <summary>
        /// Shake the reeling slider based on fishing line durability.
        /// </summary>
        /// <param name="durabilityPercent"></param>
        private void ShakeReelingSlider(float durabilityPercent)
        {
            if (_reelingSliderShakeTween.isAlive) _reelingSliderShakeTween.Complete();
            var copy = reelingSliderShakeSettings;
            copy.strength = reelingSliderShakeSettings.strength * (1 - durabilityPercent);
            copy.frequency = reelingSliderShakeSettings.frequency * (1 - durabilityPercent);
            if (copy.strength.magnitude <= 0f) return;
            _reelingSliderShakeTween = Tween.ShakeLocalPosition(reelingSlider.transform, copy);
        }
        #endregion
    }
}