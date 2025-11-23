using System;
using Madduck.Input;
using Madduck.Utils;
using R3;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Shared
{
    public class InputUIIconView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SerializableDictionary<Image, string> iconImages = new();
        [SerializeField] private SerializableDictionary<string, SpriteLibraryAsset> spriteLibraryAssets = new();
        
        private InputIconViewModel _viewModel;
        private IDisposable _binding;

        [Inject]
        public void SetUp(InputIconViewModel viewModel)
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
            if (!spriteLibraryAssets.TryGetValue(scheme, out var spriteLibraryAsset))
            {
                Debug.LogWarning($"No sprite library asset found for control scheme: {scheme}");
                return;
            }
            foreach (var (image, key) in iconImages)
            {
                var sprite = spriteLibraryAsset.GetSprite("Control", key);
                if (!sprite)
                {
                    Debug.LogWarning($"No sprite found for key: {key} in control scheme: {scheme}");
                    continue;
                }
                image.sprite = sprite;
            }
        }

        private void OnDestroy()
        {
            _binding?.Dispose();
        }
    }
}
