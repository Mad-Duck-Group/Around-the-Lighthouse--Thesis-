using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Madduck.Audio;
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
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> canvasAlphaTweenSettings;
        [SerializeField] private TweenSettings<float> backgroundAlphaTweenSettings;
        
        [Title("Audio")]
        [SerializeField] public EventReference openingSfx;
        
        [Title("Debug")]
        [Button("Preview Transition")]
        private void PreviewTransition(bool active)
        {
            Transition(active).Forget();
        }
        
        #endregion
        
        #region Fields
        
        public event Action OnOpen;
        public event Action OnClose;
        private IPlayerInputHandler _inputHandler;
        private IAudioManager _audioManager;
        private readonly List<ItemIconView> _itemIconViews = new();
        private Sequence _transitionSequence;
        private IDisposable _bindings;
        
        #endregion

        #region Injection
        public void SetUp(
            IPlayerInputHandler inputHandler,
            IAudioManager audioManager)
        {
            _inputHandler = inputHandler;
            _audioManager = audioManager;
        }
        
        public void SetPopUpObject(FishableItemPopUpObject popUpObject)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            backgroundImage.color = backgroundImage.color.WithA(backgroundAlphaTweenSettings.startValue);
            var itemCount = popUpObject.FishItemInstances.Count;
            for (var i = 0; i < itemCount; i++)
            {
                var fishItemInstance = popUpObject.FishItemInstances[i];
                var itemIconView = Instantiate(itemIconViewPrefab, layoutGroup.transform);
                itemIconView.SetItem(fishItemInstance);
                //indent even item if the number of items is odd
                if (itemCount % 2 != 0 && i % 2 != 0)
                {
                    itemIconView.SetOffset(evenItemOffset);
                }
                _itemIconViews.Add(itemIconView);
            }
        }
        #endregion

        #region Binding

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
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
            _audioManager.PlayAudioOneShot(openingSfx, Vector3.zero);
            await TransitionIn(cancellationToken);
            await _itemIconViews.Select(x => x.TransitionIn(cancellationToken));
            Bind();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            OnOpen?.Invoke();
        }

        public async UniTask Hide(CancellationToken cancellationToken = default)
        {
            _bindings?.Dispose();
            await UniTask.WhenAll(_itemIconViews.Select(x => x.TransitionOut(cancellationToken)));
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
                .Group(Tween.Alpha(canvasGroup, canvasAlphaTweenSettings.WithDirection(active)))
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