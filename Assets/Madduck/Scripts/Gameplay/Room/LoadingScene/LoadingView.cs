using System;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
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
            LoadingViewModel viewModel)
        {
            DebugUtils.Log("LoadingView SetUp");
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentDay.Subscribe(SetDay)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
            SetSea();
        }
        
        private void SetDay(uint currentDayIndex)
        {
            dayText.text = $"Day {currentDayIndex + 1}";
        }
        
        private void SetSea()
        {
            seaText.text = $"Seagull Bay";
        }
        
        private void OnDestroy()
        {
            _bindings.Dispose();
        }
    }
}
