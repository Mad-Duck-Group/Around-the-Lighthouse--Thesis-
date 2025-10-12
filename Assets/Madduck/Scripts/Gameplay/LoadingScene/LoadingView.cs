using System;
using Cysharp.Threading.Tasks;
using Madduck.Core;
using Madduck.Room;
using Madduck.Utils;
using MessagePipe;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    public class LoadingView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private TMP_Text dayText;
        [Required,
         SerializeField] private TMP_Text seaText;
        
        
        private LoadingViewModel _viewModel;
        private IDisposable _bindings;
        
        [Inject] 
        private void SetUp(
            IPublisher<LoadingSceneAnimationFinishedEvent> animationFinishedPublisher,
            LoadingViewModel viewModel)
        {
            DebugUtils.Log("LoadingView SetUp");
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentDay.Subscribe(day =>
            {
                SetDay(day);
            }).AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
            SetSea();
        }
        
        public void SetDay(uint currentDayIndex)
        {
            dayText.text = $"Day {currentDayIndex + 1}";
        }
        
        public void SetSea()
        {
            seaText.text = $"Blue Sea ";
        }
        private void OnDestroy()
        {
            _bindings.Dispose();
        }
    }
}
