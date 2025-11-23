using System;
using System.Collections.Generic;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Shared
{
    public class InputInstructionView : MonoBehaviour
    {
        [Title("References")]
        [Required,
         SerializeField] private InputInstructionIconView inputInstructionIconViewPrefab;
        [Required,
         SerializeField] private SerializableDictionary<string, SpriteLibraryAsset> spriteLibraryAssets = new();
        [Required,
         SerializeField] private LayoutGroup layoutGroup;

        private InputInstructionViewModel _viewModel;
        private readonly List<InputInstructionIconView> _iconViews = new();
        private InputInstruction[] _currentInstructions;
        private string _currentScheme;
        private IDisposable _bindings;

        [Inject]
        public void SetUp(InputInstructionViewModel viewModel)
        {
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentInstructions
                .Subscribe(OnInstructionChanged)
                .AddTo(ref disposableBuilder);
            _viewModel.CurrentScheme
                .Subscribe(OnSchemeChanged)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        private void OnInstructionChanged(InputInstruction[] instructions)
        {
            _currentInstructions = instructions;
            foreach (var view in _iconViews)
            {
                Destroy(view.gameObject);
            }
            _iconViews.Clear();
            if (!spriteLibraryAssets.TryGetValue(_currentScheme, out var spriteLibraryAsset))
            {
                DebugUtils.LogError($"No sprite library found for {_currentScheme}");
                return;
            }
            foreach (var instruction in instructions)
            {
                var iconView = Instantiate(inputInstructionIconViewPrefab, layoutGroup.transform);
                var icon = spriteLibraryAsset.GetSprite("Control", instruction.key);
                if (!icon)
                {
                    DebugUtils.LogError($"No sprite found for {instruction.key}");
                    continue;
                }
                iconView.SetUp(icon, instruction.description);
                _iconViews.Add(iconView);
            }
        }

        private void OnSchemeChanged(string scheme)
        {
            _currentScheme = scheme;
            if (!spriteLibraryAssets.TryGetValue(_currentScheme, out var spriteLibraryAsset))
            {
                DebugUtils.LogError($"No sprite library found for {_currentScheme}");
                return;
            }
            for (var i = 0; i < _iconViews.Count; i++)
            {
                var view = _iconViews[i];
                var instruction = _currentInstructions[i];
                var icon = spriteLibraryAsset.GetSprite("Control", instruction.key);
                if (!icon)
                {
                    DebugUtils.LogError($"No sprite found for {instruction.key}");
                    continue;
                }
                view.SetUp(icon, instruction.description);
            }
        }
    }
}