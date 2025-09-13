using System;
using Madduck.Scripts.Utils.Others;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Nibble
{
    public class NibbleView : MonoBehaviour
    {
        [Title("References")]
        [Required]
        [SerializeField] private Button pullHookButton;
        [Required]
        [SerializeField] private Image nibbleNotificationImage;
        
        private NibbleCommander _commander;
        private NibbleViewModel _viewModel;
        private IDisposable _isActiveSubscription;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(NibbleCommander commander, NibbleViewModel viewModel)
        {
            _commander = commander;
            _viewModel = viewModel;
            _isActiveSubscription = _viewModel.IsActive.Subscribe(SetActive);
            Bind();
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
            _isActiveSubscription.Dispose();
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