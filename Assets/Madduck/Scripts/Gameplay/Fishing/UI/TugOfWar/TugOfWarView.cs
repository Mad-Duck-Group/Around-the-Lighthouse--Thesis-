using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class TugOfWarView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required, 
         SerializeField] private Slider tugOfWarSlider;
        [Required,
         SerializeField] private Image inputIconImage;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;

        private TugOfWarViewModel _viewModel;
        private IDisposable _bindings;
        private TugOfWarUIIconConfig _tugOfWarUIIconConfig;
        private bool _isDown;
        private Sequence _transitionSequence;
        private string _currentScheme;

        [Inject]
        public void SetUp(TugOfWarViewModel viewModel, TugOfWarUIIconConfig tugOfWarUIIconConfig)
        {
            _tugOfWarUIIconConfig = tugOfWarUIIconConfig;
            _viewModel = viewModel;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.TugOfWarPercent
                .Subscribe(x => tugOfWarSlider.value = x.AsFraction)
                .AddTo(ref disposableBuilder);
            // Observable.CombineLatest(
            //         _viewModel.CurrentScheme,
            //         _viewModel.IsTugButtonDown,
            //         (scheme, isDown) => (scheme, isDown)
            //     )
            //     .ThrottleLast(TimeSpan.FromMilliseconds(300))
            //     .Subscribe(values =>
            //     {
            //         var (scheme, isDown) = values;
            //         SetIconDelay(scheme, isDown).Forget();
            //     })  
            //     .AddTo(ref disposableBuilder);
            _viewModel.CurrentScheme
                .Subscribe(scheme =>
                {
                    _currentScheme = scheme;
                    SetIconScheme(scheme);
                })
                .AddTo(ref disposableBuilder);
            _viewModel.IsTugButtonDown
                .Where(x => x)
                .SubscribeAwait((b, token) => SetIconIsDown(),AwaitOperation.Drop);
            _bindings = disposableBuilder.Build();
        }

        private void SetIconScheme(string scheme)
        {
            bool isGamepad = scheme == "Gamepad";
            SelectionIcon selectionIcon = _isDown ? SelectionIcon.Selected : SelectionIcon.Unselected;
            var sprite = _tugOfWarUIIconConfig.GetSelectedIcon(selectionIcon,isGamepad);
            inputIconImage.sprite = sprite;
        }

        private async UniTask SetIconIsDown()
        {
            _isDown = true;
            bool isGamepad = _currentScheme == "Gamepad";
            var spriteSelected = _tugOfWarUIIconConfig.GetSelectedIcon(SelectionIcon.Selected, isGamepad);
            inputIconImage.sprite = spriteSelected;
            await UniTask.WaitForSeconds(_tugOfWarUIIconConfig.iconSwitchDelay / 2);
            _isDown = false;
            var spriteUnselected = _tugOfWarUIIconConfig.GetSelectedIcon(SelectionIcon.Unselected, isGamepad);
            inputIconImage.sprite = spriteUnselected;
            await UniTask.WaitForSeconds(_tugOfWarUIIconConfig.iconSwitchDelay / 2);
            
        }
            

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
        
        private void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
            }
            else
            {
                tugOfWarSlider.value = 0f;
            }
            gameObject.SetActive(active);
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            transform.localScale = scaleTweenSettings.startValue;
            cancellationToken.Register(CancelTransition);
            SetActive(true);
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(false);
            SetActive(false);
        }
        
        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(transform, scaleTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }
        
        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }
    }
}