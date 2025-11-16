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
    public class InputIconViewModel : IStartable,IDisposable
    {
        public ReadOnlyReactiveProperty<string> CurrentScheme { get; private set; } 
        private readonly CompositeDisposable _disposables = new ();
        private readonly IPlayerInputHandler _input;
        private readonly InputIconData _data;
        
        [Inject]
        public InputIconViewModel(IPlayerInputHandler input, InputIconData data)
        {
            _data = data;
            _input = input;
            CurrentScheme = _input.CurrentControlScheme
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
        }
        public void Start()
        {
            
        }

        
        public Sprite GetIcon(InputIconType type, bool isGamepad)
        {
            if (!_data.iconMap.TryGetValue(type, out var data))
                return null;

            return isGamepad ? data.gamepadSprite : data.keyboardSprite;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }


        
    }
    
}
