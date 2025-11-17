using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.GameData
{
    public record FishableItemPopUpObject : IPopUpObject
    {
        public List<IFishableItemInstance> FishItemInstances { get; private set; }
        public FishableItemPopUpObject(List<IFishableItemInstance> FishItemInstances)
        {
            this.FishItemInstances = FishItemInstances.ToList();
        }
    }
    
    public class FishableItemPopUpView : MonoBehaviour, IPopUpView<FishableItemPopUpObject>, ITransitionable
    {
        #region Inspector

        [Title("References")]
        [Required, AssetsOnly,
         SerializeField] private ItemIconView itemIconViewPrefab;
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private Image backgroundImage;
        [Required,
         SerializeField] private LayoutGroup layoutGroup;
        [Required,
         SerializeField] private Vector2 evenItemOffset;
        // [Required,
        //  SerializeField] private TMP_Text fishNameText;
        // [Required,
        //  SerializeField] private TMP_Text fishDescriptionText;
        // [Required,
        //  SerializeField] private TMP_Text fishWeightText;
        // [Required,
        //  SerializeField] private TMP_Text fishRarityText;
        // [Required,
        //  SerializeField] private Image fishIcon;
        // [Required,
        //  SerializeField] private Button closeButton;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> backgroundAlphaTweenSettings;
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        
        #endregion
        
        #region Fields
        
        public event Action OnOpen;
        public event Action OnClose;
        private IPlayerInputHandler _inputHandler;
        private readonly List<ItemIconView> _itemIconViews = new();
        private Sequence _transitionSequence;
        private IDisposable _bindings;
        
        #endregion

        #region Injection
        public void SetUp(IPlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }
        
        public void SetPopUpObject(FishableItemPopUpObject popUpObject)
        {
            canvasGroup.transform.localScale = scaleTweenSettings.startValue;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            backgroundImage.color = backgroundImage.color.WithA(backgroundAlphaTweenSettings.startValue);
            var itemCount = popUpObject.FishItemInstances.Count;
            for (var i = 0; i < itemCount; i++)
            {
                var fishItemInstance = popUpObject.FishItemInstances[i];
                var itemIconView = Instantiate(itemIconViewPrefab, layoutGroup.transform);
                var itemInstance = fishItemInstance as IItemInstance;
                var itemData = itemInstance?.ItemData;
                if (itemData is not IItemIconData itemDisplay)
                {
                    DebugUtils.LogError($"ItemData of {fishItemInstance.GetType()} does not implement IItemIconData");
                    continue;
                }
                itemIconView.SetItem(itemDisplay);
                //indent even item if the number of items is odd
                if (itemCount % 2 != 0 && i % 2 != 0)
                {
                    itemIconView.SetOffset(evenItemOffset);
                }
                _itemIconViews.Add(itemIconView);
            }

            Bind();
        }
        #endregion

        #region Binding

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            // closeButton.OnClickAsObservable()
            //     .Subscribe(_ => OnCloseButtonClicked())
            //     .AddTo(ref disposableBuilder);
            _inputHandler.AnyButtonPressed
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => x)
                .Subscribe(_ => OnCloseButtonClicked())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        #endregion

        #region Events

        private void OnCloseButtonClicked()
        {
            Hide().Forget();
        }

        #endregion

        #region Pop Up

        public async UniTask Show(CancellationToken cancellationToken = default)
        {
            await TransitionIn(cancellationToken);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            // EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
            // Debug.Log($"current selection: {EventSystem.current.currentSelectedGameObject.name}");
            // closeButton.Select();
            OnOpen?.Invoke();
        }

        public async UniTask Hide(CancellationToken cancellationToken = default)
        {
            await TransitionOut(cancellationToken);
            Destroy(gameObject);
            OnClose?.Invoke();
        }

        #endregion

        #region Transition

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            await Transition(true, cancellationToken);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            await Transition(false, cancellationToken);
        }

        private async UniTask Transition(bool active, CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(canvasGroup.transform, scaleTweenSettings.WithDirection(active)))
                .Group(Tween.Alpha(backgroundImage, backgroundAlphaTweenSettings.WithDirection(active)));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }

        #endregion
    }
}