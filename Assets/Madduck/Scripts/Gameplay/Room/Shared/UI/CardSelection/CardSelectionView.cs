using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    public class CardSelectionView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required,
         SerializeField] private Button selectCardButton;
        [Required,
         SerializeField] private TMP_Text cardNameText;
        [Required,
         SerializeField] private Image background;
        [Required,
         SerializeField] private Image cardIcon;
        [Required,
         SerializeField] private TMP_Text cardDescriptionText;
        [Required,
         SerializeField] private TMP_Text cardRarityText;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        [SerializeField] private TweenSettings<Color> selectedColorTweenSettings;

        private CardSelectionScreenViewModel _viewModel;
        private Sequence _transitionSequence;
        private Sequence _selectCardSequence;
        private CardItemInstance _currentCard;
        private IDisposable _bindings;
        private bool _currentActiveStatus = true;
        
        public void SetUp(CardSelectionScreenViewModel viewModel)
        {
            _viewModel = viewModel;
            transform.localScale = Vector3.zero;
            Bind();
        }

        public void SetCard(CardItemInstance cardItemInstance)
        {
            _currentCard = cardItemInstance;
            cardNameText.text = cardItemInstance.GetRarityData().CardName;
            cardIcon.sprite = cardItemInstance.GetRarityData().CardIcon;
            cardDescriptionText.text = cardItemInstance.GetRarityData().CardDescription;
            cardRarityText.text = cardItemInstance.CurrentRarity.ToString();
        }

        private void Bind()
        {
            var disposableBuilder = new DisposableBuilder();
            selectCardButton.OnClickAsObservable()
                .Subscribe(_ => OnSelectCard())
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

        private void OnSelectCard()
        {
            if (_viewModel.SelectedCard.CurrentValue == _currentCard)
            {
                _viewModel.SelectCardCommand.Execute(null);
                return;
            }
            _viewModel.SelectCardCommand.Execute(_currentCard);
        }
        
        private void OnSelectedCardChanged(CardItemInstance cardItemInstance)
        {
            var selected = cardItemInstance != null && cardItemInstance == _currentCard;
            if (_currentActiveStatus == selected) return;
            _currentActiveStatus = selected;
            _selectCardSequence.Complete();
            SelectCardTransition(selected).Forget();
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(false);
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(transform, scaleTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }
        
        private async UniTask SelectCardTransition(bool forward)
        {
            _selectCardSequence = Sequence.Create()
                .Group(Tween.Color(background, selectedColorTweenSettings.WithDirection(forward)))
                .Group(Tween.Color(cardIcon, selectedColorTweenSettings.WithDirection(forward)));
                // .Group(Tween.Color(cardNameText, selectedColorTweenSettings.WithDirection(forward)))
                // .Group(Tween.Color(cardDescriptionText, selectedColorTweenSettings.WithDirection(forward)))
                // .Group(Tween.Color(cardRarityText, selectedColorTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }
    }
}
