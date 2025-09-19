using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Shared;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Fishing.UI
{
    public interface ICircleBoard
    {
        public Dictionary<FishZone, CircleBoardState> CircleBoardStates { get; }
        public void ResetCircleBoardSprite();
    }
    public class FishingBoardView : MonoBehaviour, ITransitionable, ICircleBoard
    {
        #region Inspector
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
        [SerializeField] private SerializableDictionary<Sprite, Percentage> fatigueImageDictionary = new();

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> fishingBoardScaleTweenSettings;
        [SerializeField] private TweenSettings<float> fishingBoardAlphaTweenSettings;
        [SerializeField] private ShakeSettings shakeTweenSettings;
        #endregion
        public Dictionary<FishZone, CircleBoardState> CircleBoardStates => 
            circleBoards.ToDictionary(pair => pair.Key, pair => new CircleBoardState(pair.Value));
        
        #region Fields
        private Tween _reelingSliderShakeTween;
        private List<KeyValuePair<Sprite, Percentage>> _sortedFatigueImageList = new();
        private FishingBoardViewModel _fishingBoardViewModel;
        private IDisposable _bindings;
        private Sequence _fishingBoardActivationSequence;
        private Tween _hookShakeTween;
        private Tween _fishShakeTween;
        #endregion

        #region Bindings
        [Inject]
        public void SetUp(FishingBoardViewModel fishingBoardViewModel)
        {
            _fishingBoardViewModel = fishingBoardViewModel;
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.transform.localScale = fishingBoardScaleTweenSettings.startValue;
            canvasGroup.alpha = fishingBoardAlphaTweenSettings.startValue;
        }   
        
        private void Bind()
        {
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
            DrawFishLine();
        }
        
        private void OnDestroy()
        {
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
            var sortedDictionary = fatigueImageDictionary.OrderByDescending(pair => pair.Value).ToList();
            _sortedFatigueImageList = sortedDictionary;
            foreach (var board in circleBoards)
            {
                var rectTransform = board.Value.Circle;
                rectTransform.sizeDelta = new Vector2(board.Value.Radius * 2 , board.Value.Radius * 2 );
            }
            hookObject.localPosition = circleBoards[FishZone.Red].Circle.localPosition;
        }
        
        /// <summary>
        /// Reset the circle boards to their initial sprites.
        /// </summary>
        public void ResetCircleBoardSprite()
        {
            SetFatigue(Percentage.FromPercentage(0f));
        }
        #endregion
        
        #region Transitions
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransitions);
            await Transition(true);
            SetActive(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransitions);
            await Transition(false);
            SetActive(false);
        }

        private async UniTask Transition(bool active)
        {
            _fishingBoardActivationSequence = Sequence.Create()
                .Group(Tween.Scale(canvasGroup.transform, fishingBoardScaleTweenSettings.WithDirection(active)))
                .Group(Tween.Alpha(canvasGroup, fishingBoardAlphaTweenSettings.WithDirection(active)));
                /*.OnComplete(() =>
                { 
                    if (!active) canvasGroup.gameObject.SetActive(false);
                });*/
            await _fishingBoardActivationSequence.ToYieldInstruction().ToUniTask();
        }

        private void CancelTransitions()
        {
            _fishingBoardActivationSequence.Complete();
        }
        #endregion
        
        #region Activation
        /// <summary>
        /// Set the active state of the fishing board UI.
        /// </summary>
        /// <param name="active"></param>
        private void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                fishingLineHandler.Reset();
                Cursor.lockState = CursorLockMode.None;
            }
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
        private void ShakeHook(Percentage durabilityPercent)
        {
            if (_hookShakeTween.isAlive) return;
            var copy = shakeTweenSettings;
            copy.strength = shakeTweenSettings.strength * durabilityPercent.AsInverseFraction;
            copy.frequency = shakeTweenSettings.frequency * durabilityPercent.AsInverseFraction;
            if (copy.strength.magnitude <= 0) return;
            _hookShakeTween = Tween.ShakeLocalPosition(hookIcon.transform, copy);
        }
        
        /// <summary>
        /// Shake the fish icon based on fishing line durability.
        /// </summary>
        /// <param name="durabilityPercent"></param>
        private void ShakeFish(Percentage durabilityPercent)
        {
            if (_fishShakeTween.isAlive) return;
            var copy = shakeTweenSettings;
            copy.strength = shakeTweenSettings.strength * durabilityPercent.AsInverseFraction;
            copy.frequency = shakeTweenSettings.frequency * durabilityPercent.AsInverseFraction;
            if (copy.strength.magnitude <= 0) return;
            _fishShakeTween = Tween.ShakeLocalPosition(fishIcon.transform, copy);
        }
        
        /// <summary>
        /// Set the fatigue level UI.
        /// </summary>
        /// <param name="fatiguePercent"></param>
        private void SetFatigue(Percentage fatiguePercent)
        {
            fatigueSlider.value = fatiguePercent.AsFraction;
            foreach (var pair in _sortedFatigueImageList)
            {
                if (fatiguePercent < pair.Value) continue;
                fishFatigueImage.sprite = pair.Key;
                break;
            }
        }
        #endregion
    }
    
    public class CircleBoardMock : ICircleBoard
    {
        public Dictionary<FishZone, CircleBoardState> CircleBoardStates { get; set; } = new();
        public void ResetCircleBoardSprite() { }
    }
}