using System;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Madduck.Room
{
    public class BoatSpriteView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private Transform boatTransform;
        [Required,]
        
        private BoatSpriteViewModel _spriteViewModel;
        private IDisposable _bindings;

        [Inject]
        public void SetUp(BoatSpriteViewModel spriteViewModel)
        {
            _spriteViewModel = spriteViewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _spriteViewModel.TweenSettings
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(OnTweenSettingsChanged)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
        
        private void OnTweenSettingsChanged(TweenSettings<float> tweenSettings)
        {
            var startY = boatTransform.localPosition.y; 
            var relativeSettings = tweenSettings.ToRelative(startY);
            var startDelay = Random.Range(0f, tweenSettings.settings.startDelay);
            boatTransform.position = new Vector3(boatTransform.position.x, relativeSettings.startValue, boatTransform.position.z);
            relativeSettings.settings.cycles = 1;
            relativeSettings.settings.startDelay = 0f;
            UniTask.WaitForSeconds(startDelay).ContinueWith(() =>
            {
                Sequence.Create(-1, CycleMode.Yoyo)
                    .Group(Tween.PositionY(boatTransform, relativeSettings));
            });
        }
    }
}