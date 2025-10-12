using System;
using Cysharp.Threading.Tasks;
using Madduck.Core;
using Madduck.Day;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Madduck.Room
{
    public class LoadingViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<uint> CurrentDay { get; }
        private readonly DayManager _dayManager;
        private readonly LoadSceneManager _loadSceneManager;
        private IDisposable _subscriptions;
        [Inject]
        public LoadingViewModel(DayManager dayManager,
            LoadSceneManager loadSceneManager)
        {
            _dayManager = dayManager;
            _loadSceneManager = loadSceneManager;
            CurrentDay = _dayManager.CurrentDayIndex.ToReadOnlyReactiveProperty();
            Bind();
        }

        public void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            var subscription = GlobalMessagePipe.GetSubscriber<LoadingSceneAnimationFinishedEvent>()
                .Subscribe(OnLoadingSceneAnimationFinished);
            disposableBuilder.Add(subscription);
            _subscriptions = disposableBuilder.Build();
        }

        private async void OnLoadingSceneAnimationFinished(LoadingSceneAnimationFinishedEvent _)
        {
            await new WaitForSecondsRealtime(1f);
            _loadSceneManager.LoadScene(SceneType.Gameplay, LoadSceneMode.Single, false).Forget();
        }
        
        public void Dispose()
        {
            _subscriptions?.Dispose();
        }
    }
}
