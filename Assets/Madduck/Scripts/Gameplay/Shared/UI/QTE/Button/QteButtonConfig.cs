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
                SerializeField] public UFloat EarlySuccessTimeFrame { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat SuccessTimeFrame { get; private set; }
        [field: InlineProperty, 
                SerializeField] public UFloat LateSuccessTimeFrame { get; private set; }
    }
    
    [Serializable]
    public record QteButtonConfigInstance : IStatModifiable<QteButtonConfigInstance>
    {
        [field: InlineProperty, ReadOnly, 
                ShowInInspector] public UFloat CurrentStartDelay { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentClosingInDuration { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentEarlySuccessTimeFrame { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentSuccessTimeFrame { get; set; }
        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentLateSuccessTimeFrame { get; set; }
        

        public QteButtonConfigInstance(QteButtonConfig config)
        {
                CurrentStartDelay = config.StartDelay;
                CurrentClosingInDuration = config.ClosingInDuration;
                CurrentEarlySuccessTimeFrame = config.EarlySuccessTimeFrame;
                CurrentSuccessTimeFrame = config.SuccessTimeFrame;
                CurrentLateSuccessTimeFrame = config.LateSuccessTimeFrame;
        }
        public QteButtonConfigInstance Copy() => this with {};
    }
}