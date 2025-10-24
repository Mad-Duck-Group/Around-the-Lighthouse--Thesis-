using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Shared
{
    [CreateAssetMenu(fileName = "QTEButtonConfig", menuName = "Madduck/QTE/QTE Button Config")]
    public class QTEButtonConfig : ScriptableObject
    {
        [field: InlineProperty, 
                SerializeField] public UFloat StartDelay { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat ClosingInDuration { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat SuccessTimeFrame { get; private set; }
    }
    
    [Serializable]
    public record QTEButtonConfigInstance : IStatModifiable<QTEButtonConfigInstance>
    {
        [field: InlineProperty, ReadOnly, 
                ShowInInspector] public UFloat CurrentStartDelay { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentClosingInDuration { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentSuccessTimeFrame { get; set; }
        

        public QTEButtonConfigInstance(QTEButtonConfig config)
        {
                CurrentStartDelay = config.StartDelay;
                CurrentClosingInDuration = config.ClosingInDuration;
                CurrentSuccessTimeFrame = config.SuccessTimeFrame;
        }
        public QTEButtonConfigInstance Copy() => this with {};
    }
}