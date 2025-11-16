using System;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Input
{
    public class InputUIIconView : MonoBehaviour
    {
        [SerializeField] private SerializableDictionary<InputIconType,Image> iconReferences;
        private InputIconViewModel _viewModel;
        private IDisposable _binding;

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
                .Subscribe(onNext: scheme =>
                {
                    UpdateIcon(scheme);
                }).AddTo(ref disposableBuilder);
            _binding = disposableBuilder.Build();
        }

        private void UpdateIcon(string scheme)
        {
            bool isGamepad = scheme == "Gamepad";

            foreach (var pair in iconReferences)
            {
                var sprite = _viewModel.GetIcon(pair.Key, isGamepad);
                pair.Value.sprite = sprite;
            }
        }

        private void OnDestroy()
        {
            _binding?.Dispose();
        }
    }
}
