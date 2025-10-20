using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    public class CardSelectionScreenView : MonoBehaviour, ITransitionable
    {
        [Title("References")] 
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private Button continueButton;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<float> alphaTweenSettings;

        private CardSelectionScreenViewModel _viewModel;
        private Sequence _transitionSequence;
        private IDisposable _bindings;

        [Inject]
        public void SetUp(CardSelectionScreenViewModel viewModel)
        {
            _viewModel = viewModel;
            continueButton.interactable = false;
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = new DisposableBuilder();
            continueButton.OnClickAsObservable()
                .Subscribe(_ => _viewModel.ConfirmCardCommand.Execute(Unit.Default))
                .AddTo(ref disposableBuilder);
            _viewModel.SelectedCard
                .DistinctUntilChanged()
                .Subscribe(OnSelectedCardChanged)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
        
        private void OnSelectedCardChanged(CardItemInstance cardItemInstance)
        {
            continueButton.interactable = cardItemInstance != null;
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            canvasGroup.blocksRaycasts = true;
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(false);
            canvasGroup.blocksRaycasts = false;
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Alpha(canvasGroup, alphaTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }
    }
}