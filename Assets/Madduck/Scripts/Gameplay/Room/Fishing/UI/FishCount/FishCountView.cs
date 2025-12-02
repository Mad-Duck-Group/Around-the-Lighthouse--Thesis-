using System;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class FishCountView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private TMP_Text currentFishCaughtText;
        [Required,
         SerializeField] private TMP_Text maxFishCaughtText;
        
        private FishCountViewModel _viewModel;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(FishCountViewModel viewModel)
        {
            _viewModel = viewModel;
            Bindings();
        }
        public void Bindings()
        {
            var disposableBuilder = Disposable.CreateBuilder();

            _viewModel.CurrentFishCount.Subscribe(current =>
            {
                currentFishCaughtText.text = current.ToString();
            }).AddTo(ref disposableBuilder);

            _viewModel.MaxFishCount.Subscribe(max =>
            {
                maxFishCaughtText.text = max.ToString();
            }).AddTo(ref disposableBuilder);

            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings.Dispose();
            _viewModel.Dispose();
        }

        
    }
}
