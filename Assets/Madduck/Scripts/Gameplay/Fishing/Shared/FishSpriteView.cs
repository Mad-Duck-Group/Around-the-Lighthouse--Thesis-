using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Shared
{
    public interface IFishSpriteView : ITransitionable
    {
        void SetUp(Transform hook, FishItemInstance fishItemInstance);
        void Detach();
    }
    public class FishSpriteView : MonoBehaviour, IFishSpriteView
    {
        [Title("References")]
        [Required,
         SerializeField] private SpriteRenderer spriteRenderer;
        
        [Title("Settings")] 
        [SerializeField] private TweenSettings<Vector3> scaleTween;

        [Title("Debug")] 
        [InlineEditor, 
         SerializeField] private FishItemData debugFish;
        
        private Sequence _transitionSequence;
        
        public void SetUp(Transform hook, FishItemInstance fishItemInstance)
        {
            spriteRenderer.sprite = fishItemInstance.ItemData.FishSprite;
            transform.position = hook.position;
            transform.position -= (Vector3)fishItemInstance.ItemData.SpriteAnchorOffset;
            transform.SetParent(hook);
        }

        public void Detach()
        {
            transform.SetParent(null);
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(false);
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(transform, scaleTween.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }

        private void OnDrawGizmosSelected()
        {
            if (!debugFish || !spriteRenderer) return;
            var anchoredPos = transform.position + (Vector3)debugFish.SpriteAnchorOffset;
            spriteRenderer.sprite = debugFish.FishSprite;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(anchoredPos, 0.5f);
        }
    }
}