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
        private readonly CompositeDisposable _disposables = new ();
        private readonly IPlayerInputHandler _input;
        private readonly InputIconData _Icondata;
       
        
        [Inject]
        public InputIconViewModel(IPlayerInputHandler input, InputIconData data)
        {
            _Icondata= data;
            _input = input;
            CurrentScheme = Observable.FromEvent<string>(
                        h => _input.OnControlSchemeChanged += h,
                        h => _input.OnControlSchemeChanged -= h)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(_disposables);
        }

        
        public Sprite GetIcon(InputIconType type, bool isGamepad)
        {
            if (!_Icondata.iconMap.TryGetValue(type, out var data))
                return null;

            return isGamepad ? data.gamepadSprite : data.keyboardSprite;
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }


        
    }
    
}
