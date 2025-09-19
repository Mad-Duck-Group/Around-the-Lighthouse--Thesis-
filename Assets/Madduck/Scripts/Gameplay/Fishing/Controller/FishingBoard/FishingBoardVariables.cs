using System;
using System.Collections.Generic;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    [Serializable]
    public record FishingBoardVariables
    {
        [Title("Debug"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugTitle;
        [DisplayAsString,
         ShowInInspector] public float FishPowerMultiplier { get; set; }
        [DisplayAsString,
         ShowInInspector] public float HookPowerMultiplier { get; set; }
        [DisplayAsString,
         ShowInInspector] public float AngleDifference { get; set; }
        [DisplayAsString,
         ShowInInspector] public Vector2 FishUnitCirclePosition { get; set; }
        [DisplayAsString,
         ShowInInspector] public Vector2 HookUnitCirclePosition { get; set; }
        [DisplayAsString,
         ShowInInspector] public Percentage PullPercent { get; set; }
        [DisplayAsString,
         ShowInInspector] public FishZone FishZone { get; set; }
        [DisplayAsString,
         ShowInInspector] public FishZone HookZone { get; set; }
        
        public Dictionary<FishZone, CircleBoardState> CircleBoardState { get; private set; }
        public CircleBoardState RedBoard => CircleBoardState[FishZone.Red];
        public CircleBoardState YellowBoard => CircleBoardState[FishZone.Yellow];
        public CircleBoardState GreenBoard => CircleBoardState[FishZone.Green];
        
        private readonly ICircleBoard _circleBoard;

        [Inject]
        public FishingBoardVariables(ICircleBoard circleBoard)
        {
            _circleBoard = circleBoard;
           CircleBoardState = circleBoard.CircleBoardStates;
        }

        public void ResetCircleBoardSprite()
        {
            _circleBoard.ResetCircleBoardSprite();
        }
    }
}