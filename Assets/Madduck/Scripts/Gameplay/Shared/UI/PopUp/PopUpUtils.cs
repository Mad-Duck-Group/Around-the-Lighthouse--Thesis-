using System;
using Madduck.Audio;
using Madduck.Input;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Shared
{
    public interface IPopUpObject {}

    public interface IPopUpProvider<out T> where T : IPopUpObject
    {
        public T GetPopUpObject();
    }

    public interface IPopUpView<in T> : IModal where T : IPopUpObject
    {
        public void SetPopUpObject(T popUpObject);
        public void SetUp(IPlayerInputHandler inputHandler, IAudioManager audioManager);
    }

    public interface IPopUpFactory<in T> : IFactory<IPopUpView<T>>
        where T : IPopUpObject
    {
        void DestroyPopUp();
    }
    
    [Serializable]
    public abstract class PopUpFactory<T> : IPopUpFactory<T>, IDisposable
        where T : IPopUpObject
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
        
        public IPopUpView<T> Current { get; private set; }
        protected GameObject currentPopUpViewObject;
        //protected IDisposable subscriptions;
        
        [Inject] private readonly IPlayerInputHandler _inputHandler;
        [Inject] private readonly IAudioManager _audioManager;
        
        public virtual IPopUpView<T> Create()
        {
            if (prefabMode)
            {
                Current = popUpViewPrefab.InstantiateAsInterface(new InstantiateParameters
                {
                    parent = popUpParent,
                    worldSpace = false
                }, out currentPopUpViewObject);
                currentPopUpViewObject.transform.SetAsFirstSibling();
            }
            else
            {
                Current = popUpView;
            }
            // subscriptions = Observable.FromEvent(
            //     h => Current.OnClose += h,
            //     h => Current.OnClose -= h)
            //     .Subscribe(_ => DestroyPopUp());
            Current.SetUp(_inputHandler, _audioManager);
            return Current;
        }
        
        public void DestroyPopUp()
        {
            //subscriptions?.Dispose();
            if (currentPopUpViewObject) Object.Destroy(currentPopUpViewObject);
            Current = null;
        }

        public void Dispose()
        {
            //subscriptions?.Dispose();
        }
    }
}