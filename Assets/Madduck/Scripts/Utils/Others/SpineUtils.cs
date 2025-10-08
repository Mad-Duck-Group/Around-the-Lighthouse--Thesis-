using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace Madduck.Utils
{

    [Serializable]
    public struct DeconstructedAnimationWrapper<TKey>
    {
        public DeconstructedAnimationWrapper(SkeletonDataAsset refAsset, TKey key, string animation)
        {
            this.refAsset = refAsset;
            this.key = key;
            this.animation = animation;
        }
        [HideInInspector, SerializeField] private SkeletonDataAsset refAsset;
        [ReadOnly, SerializeField] public TKey key;
        [SpineAnimation(dataField: nameof(refAsset)), 
         SerializeField] public string animation;
    }
    
    public static class SpineUtils
    {
        public static async UniTask WaitUntilComplete(this TrackEntry trackEntry, CancellationToken cancellationToken = default)
        {
            if (trackEntry is null) return;
            if (trackEntry.IsComplete) return;
            if (trackEntry.Loop)
            {
                DebugUtils.LogWarning("Track entry is looped, cannot wait until complete");
                return;
            }
            var tcs = new UniTaskCompletionSource();
            cancellationToken.Register(() => tcs.TrySetCanceled());
            trackEntry.Complete += _ => tcs.TrySetResult();
            trackEntry.Dispose += _ => tcs.TrySetCanceled();
            await tcs.Task;
        }
    }
}