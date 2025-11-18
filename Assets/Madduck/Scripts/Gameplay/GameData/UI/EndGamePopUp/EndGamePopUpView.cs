using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.GameData
{
    public record EndGamePopUpObject : IPopUpObject
    {
        public EndGamePopUpObject()
        {
            
        }
    }
    public class EndGamePopUpView : MonoBehaviour, IPopUpView<EndGamePopUpObject>, ITransitionable
    {
        #region Inspector

        [Title("References")]
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private Image backgroundImage;
        [Required,
         SerializeField] private TMP_Text endGameText;

        [Title("Settings")] 
        [SerializeField] private List<string> endGameMessages = new();
        [SerializeField] private float startDelay = 2f;
        [SerializeField] private float messageStayDuration = 2f;
        [SerializeField] private float endDelay = 2f;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> canvasAlphaTweenSettings;
        [SerializeField] private TweenSettings<float> backgroundAlphaTweenSettings;
        [SerializeField] private TweenSettings<float> textAlphaTweenSettings;
        
        #endregion
        
        #region Fields
        
        public event Action OnOpen;
        public event Action OnClose;
        private IPlayerInputHandler _inputHandler;
        private Sequence _transitionSequence;
        private IDisposable _bindings;
        
        #endregion

        #region Injection
        public void SetUp(IPlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }
        
        public void SetPopUpObject(EndGamePopUpObject popUpObject)
        {
            canvasGroup.alpha = canvasAlphaTweenSettings.startValue;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            backgroundImage.color = backgroundImage.color.WithA(backgroundAlphaTweenSettings.startValue);
            endGameText.color = endGameText.color.WithA(textAlphaTweenSettings.startValue);
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
            return; // Disable manual close for end game pop up
            Hide().Forget();
        }

        #endregion

        #region Pop Up

        public async UniTask Show(CancellationToken cancellationToken = default)
        {
            await TransitionIn(cancellationToken);
            Bind();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            OnOpen?.Invoke();
            ShowMessage(cancellationToken).Forget();
        }

        public async UniTask Hide(CancellationToken cancellationToken = default)
        {
            await TransitionOut(cancellationToken);
            Destroy(gameObject);
            OnClose?.Invoke();
        }

        #endregion

        private async UniTaskVoid ShowMessage(CancellationToken cancellationToken = default)
        {
            await UniTask.WaitForSeconds(startDelay, cancellationToken: cancellationToken);
            for (var i = 0; i < endGameMessages.Count; i++)
            {
                var message = endGameMessages[i];
                endGameText.text = message;
                var textTransitionSequence = Sequence.Create()
                    .Group(Tween.Alpha(endGameText, textAlphaTweenSettings.WithDirection(true)));
                if (i < endGameMessages.Count - 1)
                {
                    _ = textTransitionSequence
                            .ChainDelay(messageStayDuration) 
                            .Chain(Tween.Alpha(endGameText, textAlphaTweenSettings.WithDirection(false)));
                }
                await textTransitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
            }
            await UniTask.WaitForSeconds(endDelay, cancellationToken: cancellationToken);
            Hide(cancellationToken).Forget();
        }

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