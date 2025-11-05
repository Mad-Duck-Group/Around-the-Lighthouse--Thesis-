using PrimeTween;
using UnityEngine;

namespace HasanSadikin.Carousel
{
    public static class UIUtils 
    {
        public static Sequence CreateSequence(this MonoBehaviour mono, ref Sequence? prevSeq)
        {
            // Stop the previous running sequence
            if (prevSeq.HasValue && prevSeq.Value.isAlive)
            {
                prevSeq.Value.Stop();
            }

            // Create new sequence
            prevSeq = Sequence.Create();

            return prevSeq.Value;
        }
    }
}