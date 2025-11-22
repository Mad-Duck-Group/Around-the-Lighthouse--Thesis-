using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Shared
{
    [CreateAssetMenu(fileName = "QTESequenceConfig", menuName = "Madduck/QTE/QTE Sequence Config")]
    public class QteSequenceConfig : ScriptableObject
    {
        [field: InlineProperty, 
                SerializeField] public UFloat StartDelay { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat ActivationDelay { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat Interval { get; private set; }
        [field: SerializeField] public uint SequenceLength { get; private set; }
    }
    
    [Serializable]
    public record QteSequenceConfigInstance : IStatModifiable<QteSequenceConfigInstance>
    {
        [field: DisplayAsString] public UFloat CurrentStartDelay { get; private set; }
        [field: DisplayAsString] public UFloat CurrentInterval { get; private set; }
        [field: DisplayAsString] public UFloat CurrentActivationDelay { get; private set; }
        [field: DisplayAsString] public uint CurrentSequenceLength { get; private set; }
        
        public QteSequenceConfigInstance(QteSequenceConfig config)
        {
            CurrentStartDelay = config.StartDelay;
            CurrentInterval = config.Interval;
            CurrentActivationDelay = config.ActivationDelay;
            CurrentSequenceLength = config.SequenceLength;
        }

        public QteSequenceConfigInstance Copy() => this with {};
    }
}