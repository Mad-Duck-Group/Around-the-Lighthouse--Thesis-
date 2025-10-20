using System;
using Cysharp.Threading.Tasks;
using Madduck.Core;
using Madduck.Day;
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
        private readonly LoadSceneManager _loadSceneManager;
        private readonly ISubscriber<LoadingSceneAnimationFinishedEvent> _loadingSceneAnimationFinishedEventSubscriber;
        private IDisposable _subscriptions;
        
        [Inject]
        public LoadingViewModel(
            DayManager dayManager,
            LoadSceneManager loadSceneManager,
            ISubscriber<LoadingSceneAnimationFinishedEvent> loadingSceneAnimationFinishedEventSubscriber)

        {
            _loadSceneManager = loadSceneManager;
            CurrentDay = dayManager.CurrentDayIndex.ToReadOnlyReactiveProperty();
            _loadingSceneAnimationFinishedEventSubscriber = loadingSceneAnimationFinishedEventSubscriber;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            var subscription = _loadingSceneAnimationFinishedEventSubscriber
                .Subscribe(_ => OnLoadingSceneAnimationFinished().Forget());
            disposableBuilder.Add(subscription);
            _subscriptions = disposableBuilder.Build();
        }

        private async UniTaskVoid OnLoadingSceneAnimationFinished()
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
