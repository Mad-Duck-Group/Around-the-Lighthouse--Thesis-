using System;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Input
{
    public class InputUIIconView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SerializableDictionary<InputIconType,Image> iconImages;

        
        private InputIconViewModel _viewModel;
        private IDisposable _binding;
        private AnimationClip _pendingClip;

        [Inject]
        public void Construct(InputIconViewModel viewModel)
        {
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentScheme
                .Subscribe(onNext: UpdateIcon)
                .AddTo(ref disposableBuilder);
            _binding = disposableBuilder.Build();
        }

        private void UpdateIcon(string scheme)
        {
            bool isGamepad = scheme == "Gamepad";
            foreach (var pair in iconImages)
            {
                var sprite = _viewModel.GetIcon(pair.Key, isGamepad); pair.Value.sprite = sprite;
            }
            
        }

        private void OnDestroy()
        {
            _binding?.Dispose();
        }
    }
}
