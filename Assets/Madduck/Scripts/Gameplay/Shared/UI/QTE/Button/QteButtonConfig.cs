using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Shared
{
    [CreateAssetMenu(fileName = "QTEButtonConfig", menuName = "Madduck/QTE/QTE Button Config")]
    public class QteButtonConfig : ScriptableObject
    {
        [field: InlineProperty, 
                SerializeField] public UFloat StartDelay { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat ClosingInDuration { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat SuccessTimeFrame { get; private set; }
    }
    
    [Serializable]
    public record QteButtonConfigInstance : IStatModifiable<QteButtonConfigInstance>
    {
        [field: InlineProperty, ReadOnly, 
                ShowInInspector] public UFloat CurrentStartDelay { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentClosingInDuration { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentSuccessTimeFrame { get; set; }
        

        public QteButtonConfigInstance(QteButtonConfig config)
        {
                CurrentStartDelay = config.StartDelay;
                CurrentClosingInDuration = config.ClosingInDuration;
                CurrentSuccessTimeFrame = config.SuccessTimeFrame;
        }
        public QteButtonConfigInstance Copy() => this with {};
    }
}