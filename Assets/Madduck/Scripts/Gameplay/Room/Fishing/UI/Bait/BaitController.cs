using System;
using Madduck.Input;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    public class BaitController : IDisposable ,IStartable
    {
        private readonly PlayerInputHandler _inputHandler;
        private IDisposable _baitBinding;

        [Inject]
        public BaitController(PlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;

        }
        public void Start()
        {
            Bind();
            DebugUtils.Log("test start");

        }
        private void Bind()
        {
            _baitBinding = _inputHandler.BaitButton.IsDown.Where(x => x)
                .Subscribe(_ => { OpenUI();});
            //DebugUtils.Log("test inject");
        }

        private void OpenUI()
        {
            DebugUtils.Log("OpenUI");
        }

        public void Dispose()
        {
            _baitBinding?.Dispose();
        }


        
    }
}
