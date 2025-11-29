using System;
using Madduck.Core;
using Madduck.Day;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Save;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Room
{
    public class FishingRoomSecretResetHandler : IDisposable
    {
        private readonly FishingRoomManager _fishingRoomManager;
        private readonly MessagePackSaveManager _saveManager;
        private readonly DayManager _dayManager;
        private readonly FishCatalogue _fishCatalogue;
        private readonly IPlayerInputHandler _inputHandler;
        
        private IDisposable _subscriptions;
        
        [Inject]
        public FishingRoomSecretResetHandler(
            FishingRoomManager fishingRoomManager,
            MessagePackSaveManager saveManager,
            DayManager dayManager,
            FishCatalogue fishCatalogue, 
            IPlayerInputHandler inputHandler)
        {
            _fishingRoomManager = fishingRoomManager;
            _saveManager = saveManager;
            _dayManager = dayManager;
            _fishCatalogue = fishCatalogue;
            _inputHandler = inputHandler;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.SecretResetButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => x)
                .Subscribe(_ => OnSecretReset())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions.Dispose();
        }
        
        private void OnSecretReset()
        {
            _saveManager.ResetAll();
            _saveManager.SaveAll();
            _fishCatalogue.Reset();
            _dayManager.SetDayIndex(0);
            _fishingRoomManager.ToMainMenu();
        }
    }
}