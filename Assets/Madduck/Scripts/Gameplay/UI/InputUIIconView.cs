using System;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Input
{
    public class InputUIIconView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SerializableDictionary<InputIconType,Image> iconImages;

        [SerializeField] private SerializableDictionary<InputIconType, Animator> animators;
        
        private InputIconViewModel _viewModel;
        private IDisposable _binding;
        private AnimationClip _pendingClip;

        [Inject]
        public void Construct(InputIconViewModel viewModel)
        {
            DebugUtils.Log("vm injected to InputUIIconView");
            _viewModel = viewModel;
            Bind();
        }
        

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CurrentScheme
                .Subscribe(onNext: scheme =>
                {
                    UpdateIcon(scheme);
                    //UpdateAnimation(scheme);

                }).AddTo(ref disposableBuilder);
            _binding = disposableBuilder.Build();
        }

        private void UpdateIcon(string scheme)
        {
            bool isGamepad = scheme == "Gamepad";
            foreach (var pair in iconImages)
            {
                var sprite = _viewModel.GetIcon(pair.Key, isGamepad); pair.Value.sprite = sprite;
            }
            
        }
        private void UpdateAnimation(string scheme)
        {
            // bool isGamepad = scheme == "Gamepad";
            // foreach (var pair in animatorReferences) 
            // {
            //     var type = pair.Key;
            //     var animator = pair.Value;
            //
            //     var clip = _viewModel.GetAnimation(type, isGamepad);
            //
            //     if (clip == null)
            //     {
            //         continue;
            //     }
            //     PlayAnimation(animator, clip);
            // }
        }

        private void PlayAnimation(Animator animator, AnimationClip clip)
        {
            if (!animator.gameObject.activeInHierarchy)
            {
                _pendingClip = clip; 
                return;
            }
            if (!(animator.runtimeAnimatorController is AnimatorOverrideController overrideController))
            {
                overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                animator.runtimeAnimatorController = overrideController;
            }

            overrideController["Default"] = clip;
            animator.Play("Default", 0, 0f);
            animator.Update(0f);
        }

        private void OnDestroy()
        {
            _binding?.Dispose();
        }
    }
}
