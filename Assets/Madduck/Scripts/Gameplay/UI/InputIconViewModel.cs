using System;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Input
{
    public class InputIconViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<string> CurrentScheme { get; private set; } 
        private readonly IPlayerInputHandler _input;
        private readonly InputIconData _icondata;
        private IDisposable _disposables;
        
        [Inject]
        public InputIconViewModel(
            IPlayerInputHandler input, 
            InputIconData data)
        {
            _icondata= data;
            _input = input;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            CurrentScheme = Observable.FromEvent<string>(
                    h => _input.OnControlSchemeChanged += h,
                    h => _input.OnControlSchemeChanged -= h)
                .ToReadOnlyReactiveProperty(_input.CurrentControlScheme)
                .AddTo(ref disposableBuilder);
            _disposables = disposableBuilder.Build();
        }

        
        public Sprite GetIcon(InputIconType type, bool isGamepad)
        {
            if (!_icondata.iconMap.TryGetValue(type, out var data))
                return null;

            return isGamepad ? data.gamepadSprite : data.keyboardSprite;
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
    
}
