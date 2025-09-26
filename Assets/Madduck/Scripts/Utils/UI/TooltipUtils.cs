using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Utils
{
    
    public interface ITooltipObject { }

    public interface ITooltipView<in T> where T : ITooltipObject
    {
        public UniTask ShowTooltip(T tooltip, CancellationToken cancellationToken = default);
        public UniTask HideTooltip(CancellationToken cancellationToken = default);
    }

    public record GeneralTooltipObject(string Title, string Description) : ITooltipObject
    {
        public string Title { get; private set; } = Title;
        public string Description { get; private set; } = Description;
    }

    [Serializable]
    public abstract class TooltipManager<T> where T : ITooltipObject
    {
        [Title("References")]
        [Required, 
         SerializeField] protected Transform tooltipParent;
        [field: SerializeField] public Canvas TooltipCanvas { get; set; }
        [SerializeField] protected bool prefabMode;
        [Required, HideIf(nameof(prefabMode)),
         OdinSerialize] protected ITooltipView<T> tooltipView;
        [Required, ShowIf(nameof(prefabMode)),
         OdinSerialize] protected ITooltipView<T> tooltipViewPrefab;

        [Title("Settings")] 
        [SerializeField] protected float delay = 1f;
        [SerializeField] protected RectTransformInset canvasMargin;
        [SerializeField] protected Vector2 offset;
        
        private ITooltipView<T> _currentTooltipView; 
        private GameObject _currentTooltipViewObject;

        public virtual async UniTask ShowTooltip(ITooltipObject tooltipObject, CancellationToken cancellationToken = default)
        {
            if (!TooltipCanvas)
            {
                DebugUtils.LogError("TooltipCanvas is not assigned");
                return;
            }
            if (tooltipObject is not T data)
            {
                DebugUtils.LogError($"TooltipObject is not of type {typeof(T).Name}");
                return;
            }
            await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);
            if (prefabMode)
            {
                _currentTooltipView = tooltipViewPrefab.InstantiateAsInterface(new InstantiateParameters
                {
                    parent = tooltipParent,
                    worldSpace = false
                }, out _currentTooltipViewObject);
                var tooltipRectTransform = (RectTransform)_currentTooltipViewObject.transform;
                var canvasRectTransform = (RectTransform)TooltipCanvas.transform;
                tooltipRectTransform.localPosition += (Vector3)offset;
                tooltipRectTransform.ClampTo(canvasRectTransform, canvasMargin);
            }
            else
            {
                _currentTooltipView = tooltipView;
            }
            await _currentTooltipView.ShowTooltip(data, cancellationToken);
        }
        
        public virtual async UniTask HideTooltip(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(DestroyTooltipObject);
            if (_currentTooltipView != null)
                await _currentTooltipView.HideTooltip(cancellationToken);
            DestroyTooltipObject();
        }

        protected virtual void DestroyTooltipObject()
        {
            if (_currentTooltipViewObject) Object.Destroy(_currentTooltipViewObject);
            _currentTooltipView = null;
        }
    }
}