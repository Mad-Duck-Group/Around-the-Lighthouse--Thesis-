using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Utils
{
    public class OrthographicCameraManager : IDisposable
    {
        [Title("Debug"),
            HideLabel,
            ShowInInspector] private InspectorPlaceholder _debugPlaceholder;
        
        private readonly Camera _mainCamera;
        public Camera MainCamera => _mainCamera;
        
        private Sequence _shakeSequence;

        [Inject]
        public OrthographicCameraManager(Camera mainCamera)
        {
            _mainCamera = mainCamera;
        }

        public void Dispose()
        {
            
        }

        [HideInEditorMode,
         Button("Shake")]
        public UniTask Shake(ShakeSettings shakeSettings, float strengthFactor)
        {
            _shakeSequence = Sequence.Create()
                .Group(Tween.ShakeCamera(_mainCamera, strengthFactor, shakeSettings.duration, shakeSettings.frequency,
                    shakeSettings.startDelay, shakeSettings.endDelay, shakeSettings.useUnscaledTime));
            return _shakeSequence.ToYieldInstruction().ToUniTask();
        }
        
        [HideInEditorMode,
         Button("Stop Shake")]
        public void StopShake(bool complete = true)
        {
            if (complete)
            {
                _shakeSequence.Complete();
            }
            else
            {
                _shakeSequence.Stop();
            }
        }

        [HideInEditorMode,
         Button("Change Orthographic Size")]
        public UniTask ChangeOrthographicSize(TweenSettings<float> tweenSettings)
        {
            var sequence = Sequence.Create()
                .Group(Tween.CameraOrthographicSize(_mainCamera, tweenSettings));
            return sequence.ToYieldInstruction().ToUniTask();
        }
    }
    
    [Serializable]
    public record OrthographicCameraManagerDebugData : IDebugData
    {
        [field: SerializeField] public bool ConstantUpdate { get; private set; }
        [field: SerializeField] public bool AutoCloseWhenPlayModeEnds { get; private set; }
        [ShowInInspector] private OrthographicCameraManager _manager;
        
        public OrthographicCameraManagerDebugData(
            OrthographicCameraManager manager)
        {
            ConstantUpdate = false;
            AutoCloseWhenPlayModeEnds = true;
            _manager = manager;
        }
    }

    [Serializable]
    public class OrthographicCameraManagerInstaller : IInstaller
    {
        [Title("Camera Manager")] 
        [Required, 
         SerializeField] private Camera mainCamera;
        
#if UNITY_EDITOR
        [HideInEditorMode]
        [Button("Open Debug Window")]
        private void OpenDebugWindow()
        {
            DebugEditorWindow.Inspect(_orthographicCameraManagerDebugData, "Orthographic Camera Manager Debug");
        }

        private OrthographicCameraManagerDebugData _orthographicCameraManagerDebugData;
#endif
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(mainCamera).AsSelf();
            builder.Register<OrthographicCameraManager>(Lifetime.Singleton).AsSelf();
            builder.RegisterBuildCallback(x =>
            {
                var manager = x.Resolve<OrthographicCameraManager>();
#if UNITY_EDITOR
                _orthographicCameraManagerDebugData = new OrthographicCameraManagerDebugData(manager);
#endif
            });
        }
    }
}