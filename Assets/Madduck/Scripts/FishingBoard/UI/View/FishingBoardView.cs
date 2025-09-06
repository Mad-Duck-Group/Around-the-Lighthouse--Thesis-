using System;
using Madduck.Scripts.FishingBoard.UI.ViewModel;
using PrimeTween;
using R3;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.FishingBoard.UI.View
{
    public class FishingBoardView : MonoBehaviour
    {
        [Title("References")] 
        [SerializeField] private CanvasGroup canvasGroup;
        [field:SerializeField] public RectTransform HookObject { get; private set; }
        [SerializeField] private RectTransform hookIcon;
        [field: SerializeField] public RectTransform FishObject { get; private set; }
        [SerializeField] private RectTransform fishIcon;
        [SerializeField] private FishingLineHandler fishingLineHandler;
        [field: SerializeField] public SerializableDictionary<FishZone, CircleBoard> CircleBoards { get; private set; } = new();

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> fishingBoardScaleTweenSettings;
        [SerializeField] private TweenSettings<float> fishingBoardAlphaTweenSettings;
        [SerializeField] private ShakeSettings shakeTweenSettings;

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
            Bind();
        }
        
        private void Bind()
        {
            _bindings?.Dispose();
            var fishPositionBinding = 
                _fishingBoardViewModel.FishPosition
                .Subscribe(x =>
                {
                    FishObject.localPosition = x;
                    ClampPosition(FishObject);
                    DrawFishLine();
                });
            var fishRotationBinding = 
                _fishingBoardViewModel.FishRotation
                .Subscribe(x => FishObject.localRotation = x);
            var hookPositionBinding = 
                _fishingBoardViewModel.HookPosition
                .Subscribe(x =>
                {
                    HookObject.localPosition = x;
                    ClampPosition(HookObject);
                    DrawFishLine();
                });
            var hookRotationBinding = 
                _fishingBoardViewModel.HookRotation
                .Subscribe(x => HookObject.localRotation = x);
            var durabilityBinding = 
                _fishingBoardViewModel.FishLineDurabilityPercent
                .Subscribe(x =>
                {
                    ShakeHook(x);
                    ShakeFish(x);
                    SetTension(x);
                });
            _bindings = Disposable.Combine(
                fishPositionBinding, 
                fishRotationBinding, 
                hookPositionBinding, 
                hookRotationBinding, 
                durabilityBinding);
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
            _isActiveBinding?.Dispose();
            _bindings?.Dispose();
            _fishingBoardViewModel.Dispose();
        }
        
        private void InitializeFishingBoard()
        {
            foreach (var board in CircleBoards)
            {
                var rectTransform = board.Value.Circle;
                rectTransform.sizeDelta = new Vector2(board.Value.Radius * 2 , board.Value.Radius * 2 );
            }
            //Set Hook
            if (HookObject == null)
            {
                return;
            }
            HookObject.localPosition = CircleBoards[FishZone.Red].Circle.localPosition;
        }
        #endregion
        
        #region UI
        private void SetActive(bool active)
        {
            if (active)
            {
                Bind();
                canvasGroup.gameObject.SetActive(true);
            }
            else
            {
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
        private void DrawFishLine()
        {
            var center = CircleBoards[FishZone.Red].Circle;
            fishingLineHandler.GetWidthHeight( CircleBoards[FishZone.Red].Radius * 2, CircleBoards[FishZone.Red].Radius * 2);
            fishingLineHandler.SetPoints(HookObject.transform, center.transform, FishObject.transform);
        }
        
        private void ShakeHook(float durability)
        {
            if (_hookShakeTween.isAlive) return;
            var copy = shakeTweenSettings;
            copy.strength = shakeTweenSettings.strength * (1 - durability);
            copy.frequency = shakeTweenSettings.frequency * (1 - durability);
            if (copy.strength.magnitude <= 0) return;
            _hookShakeTween = Tween.ShakeLocalPosition(hookIcon.transform, copy);
        }
        
        private void ShakeFish(float durability)
        {
            if (_fishShakeTween.isAlive) return;
            var copy = shakeTweenSettings;
            copy.strength = shakeTweenSettings.strength * (1 - durability);
            copy.frequency = shakeTweenSettings.frequency * (1 - durability);
            if (copy.strength.magnitude <= 0) return;
            _fishShakeTween = Tween.ShakeLocalPosition(fishIcon.transform, copy);
        }
        
        private void ClampPosition(Transform target)
        {
            var redBoard = CircleBoards[FishZone.Red];
            var centerToPosition = (redBoard.Circle.localPosition - target.localPosition).normalized;
            var maxMagnitude = redBoard.Radius * centerToPosition.magnitude;
            target.localPosition = Vector3.ClampMagnitude(target.localPosition, maxMagnitude);
        }
        
        private void SetTension(float percentDurability)
        {
            if (percentDurability <= 0.3)
            {
                fishingLineHandler.FishLineTension = FishLineTension.High;
            }
            else if (percentDurability <= 0.5)
            {
                fishingLineHandler.FishLineTension = FishLineTension.Medium;
            }
            else if (percentDurability <= 0.7)
            {
                fishingLineHandler.FishLineTension = FishLineTension.Low;
            }
            else
            {
                fishingLineHandler.FishLineTension = FishLineTension.Normal;
            }
        }
        
        #endregion
    }
}