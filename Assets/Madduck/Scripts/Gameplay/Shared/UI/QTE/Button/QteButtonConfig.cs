using System;
using FMODUnity;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Shared
{
    [CreateAssetMenu(fileName = "QTEButtonConfig", menuName = "Madduck/QTE/QTE Button Config")]
    public class QteButtonConfig : ScriptableObject
    { 
            [Title("Qte"),
       HideLabel,
       ShowInInspector] private InspectorPlaceholder _qteTitle;
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
        
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference QtePressSfx { get; private set; }
    }
    
    [Serializable]
    public record QteButtonConfigInstance(QteButtonConfig BaseConfig) : IStatModifiable<QteButtonConfigInstance>
    {
        [field: InlineProperty, ReadOnly, 
                ShowInInspector] public UFloat CurrentStartDelay { get; set; } = BaseConfig.StartDelay;

        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentClosingInDuration { get; set; } = BaseConfig.ClosingInDuration;

        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentEarlySuccessTimeFrame { get; set; } = BaseConfig.EarlySuccessTimeFrame;

        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentSuccessTimeFrame { get; set; } = BaseConfig.SuccessTimeFrame;

        [field: InlineProperty, ReadOnly,
                ShowInInspector] public UFloat CurrentLateSuccessTimeFrame { get; set; } = BaseConfig.LateSuccessTimeFrame;

        public QteButtonConfig BaseConfig { get; private set; } = BaseConfig;

        public QteButtonConfigInstance Copy() => this with {};
    }
}