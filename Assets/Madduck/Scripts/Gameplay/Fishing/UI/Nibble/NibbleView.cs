using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class NibbleView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required]
        [SerializeField] private Button pullHookButton;
        [Required]
        [SerializeField] private Image nibbleNotificationImage;
        
        private NibbleCommander _commander;
        private NibbleViewModel _viewModel;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(NibbleCommander commander, NibbleViewModel viewModel)
        {
            _commander = commander;
            _viewModel = viewModel;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.IsNibbling
                .Subscribe(OnNibble)
                .AddTo(ref disposableBuilder);
            pullHookButton.onClick
                .AsObservable()
                .Subscribe(_ => OnPullHook())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
    
        #region Transitions
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            //await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken); // placeholder for actual transition animation
            SetActive(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            //await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken); // placeholder for actual transition animation
            SetActive(false);
        }
        
        private void CancelTransition()
        {
            // Implement if needed
        }
        #endregion

        private void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
            }
            else
            {
                OnNibble(false);
            }
            gameObject.SetActive(active);
        }

        private void OnNibble(bool isNibbling)
        {
            nibbleNotificationImage.gameObject.SetActive(isNibbling);
        }
        
        private void OnPullHook()
        {
            _commander.PullHookCommand.Execute(Unit.Default);
        }
    }
}