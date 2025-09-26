using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Madduck.Utils
{
    public interface IPopUpObject {}

    public interface IPopUpProvider<out T> where T : IPopUpObject
    {
        public T GetPopUpObject();
    }

    public interface IPopUpView<T> where T : IPopUpObject
    {
        public void SetUp(PopUpManager<T> popUpManager);
        public UniTask ShowPopUp(T popUpObject, CancellationToken cancellationToken = default);
        public UniTask HidePopUp(CancellationToken cancellationToken = default);
    }

    public interface IPopUpManager
    {
        UniTask ShowPopUp(IPopUpObject popUpObject, CancellationToken cancellationToken = default);
        UniTask HidePopUp(CancellationToken cancellationToken = default);
        public event Action OnPopUpShown;
        public event Action OnPopUpHidden;
    }

    [Serializable]
    public abstract class PopUpManager<T> : IPopUpManager where T : IPopUpObject
    {
        [Title("References")]
        [Required, 
         SerializeField] protected Transform popUpParent;
        [field: SerializeField] public Canvas PopUpCanvas { get; set; }
        [SerializeField] protected bool prefabMode;
        [Required, HideIf(nameof(prefabMode)),
         OdinSerialize] protected IPopUpView<T> popUpView;
        [Required, ShowIf(nameof(prefabMode)),
         OdinSerialize] protected IPopUpView<T> popUpViewPrefab;
        
        public event Action OnPopUpShown;
        public event Action OnPopUpHidden;

        protected IPopUpView<T> currentPopUpView;
        protected GameObject currentPopUpViewObject;
        
        public virtual async UniTask ShowPopUp(IPopUpObject popUpObject, CancellationToken cancellationToken = default)
        {
            if (!PopUpCanvas)
            {
                DebugUtils.LogError("PopUpCanvas is not assigned");
                return;
            }
            if (popUpObject is not T data)
            {
                DebugUtils.LogError("PopUpObject is not of type T");
                return;
            }
            if (prefabMode)
            {
                currentPopUpView = popUpViewPrefab.InstantiateAsInterface(new InstantiateParameters
                {
                    parent = popUpParent,
                    worldSpace = false
                }, out currentPopUpViewObject);
            }
            else
            {
                currentPopUpView = popUpView;
            }
            currentPopUpView.SetUp(this);
            OnPopUpShown?.Invoke();
            await currentPopUpView.ShowPopUp(data, cancellationToken);
        }
        
        public virtual async UniTask HidePopUp(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(DestroyPopUpObject);
            if (currentPopUpView != null)
                await currentPopUpView.HidePopUp(cancellationToken);
            OnPopUpHidden?.Invoke();
            DestroyPopUpObject();
        }

        protected virtual void DestroyPopUpObject()
        {
            if (currentPopUpViewObject) Object.Destroy(currentPopUpViewObject);
            currentPopUpView = null;
        }
    }
}