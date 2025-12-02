using System;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class DayCountView : MonoBehaviour
    {
        [Title("Reference")]
        [Required,
         SerializeField] private TMP_Text dayCountText;
        
        private DayCountViewModel _viewModel;
        private IDisposable _bindings;

        [Inject]
        public void SetUp(DayCountViewModel viewModel)
        {
            _viewModel = viewModel;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentDayIndex
                .Subscribe(OnDayIndexChanged)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
        
        private void OnDayIndexChanged(uint dayIndex)
        {
            dayCountText.text = $"Day {dayIndex + 1}";
        }
    }
}