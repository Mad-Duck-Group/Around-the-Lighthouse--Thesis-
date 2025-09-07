using System;
using Madduck.Scripts.Audio;
using Madduck.Scripts.FishingBoard.UI.Model;
using R3;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using VContainer.Unity;

namespace Madduck.Scripts.FishingBoard
{
    /// <summary>
    /// State of the Fishing Board mini-game.
    /// </summary>
    [Serializable]
    public class FishingBoardState : IStartable, IDisposable
    {
        #region Inspector
        [Button("Test Start")]
        public void TestStart() => StartFishingBoard();
        #endregion

        #region Fields & Properties
        private readonly FishingBoardConfig _fishingBoardConfig;
        private readonly FishingBoardController _fishingBoardController;
        private readonly BehaviorGraphAgent _behaviorGraphAgent;
        private readonly AudioManager _audioManager;
        private readonly FishingBoardModel _model;
        private IDisposable _updateSubscription;
        
        #region Blackboard Variables
        private BlackboardVariable<BackboardFishZone> _fishZone;
        private BlackboardVariable<BackboardFishZone> _hookZone;
        private BlackboardVariable<Vector2> _fishUnitCirclePosition;
        private BlackboardVariable<Vector2> _hookUnitCirclePosition;
        private BlackboardVariable<float> _angleDifference;
        private BlackboardVariable<float> _fatiguePercent;
        #endregion
        
        private AudioReference _fishingLineTensionAudioReference;
        #endregion
        
        #region Injection
        public FishingBoardState(
            FishingBoardConfig fishingBoardConfig,
            FishingBoardController fishingBoardController, 
            BehaviorGraphAgent behaviorGraphAgent, 
            AudioManager audioManager,
            FishingBoardModel model)
        {
            _fishingBoardConfig = fishingBoardConfig;
            _fishingBoardController = fishingBoardController;
            _behaviorGraphAgent = behaviorGraphAgent;
            _audioManager = audioManager;
            _model = model;
        }
        #endregion

        #region Lifecycle
        public void Start()
        {
            _fishingBoardController.SetActive(false);
        }

        public void Dispose()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;
        }
        #endregion

        #region Activation
        /// <summary>
        /// Start the fishing board mini-game.
        /// </summary>
        private void StartFishingBoard()
        {
            if (_updateSubscription != null) return;
            _fishingBoardController.SetActive(true);
            ResetFatigueLevel();
            InitializeBehaviorGraph();
            _model.MaxFatigueLevel.Value = _fishingBoardConfig.MaxFatigueLevel;
            _fishingLineTensionAudioReference = _audioManager.PlayAudio(_fishingBoardConfig.FishingLineTensionSfx, Vector3.zero);
            _updateSubscription = Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => Update());
        }

        /// <summary>
        /// Update the fishing board state. Called every frame until the mini-game ends.
        /// </summary>
        public void Update()
        {
            UpdateFatigueLevel();
            UpdateFishingLineDurability();
            UpdateBehaviourGraphVariables();
        }

        /// <summary>
        /// Stop the fishing board mini-game.
        /// </summary>
        public void StopFishingBoard()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;
            ShutdownBehaviorGraph();
            _behaviorGraphAgent.enabled = false;
            _model.CurrentFatigueLevel.Value = _fishingBoardConfig.MaxFatigueLevel / 2;
            _audioManager.StopAudio(_fishingLineTensionAudioReference);
            _fishingBoardController.SetActive(false);
        }
        #endregion

        #region Fishing Board
        /// <summary>
        /// Initialize the behavior graph for fish behavior.
        /// </summary>
        private void InitializeBehaviorGraph()
        {
            _behaviorGraphAgent.enabled = true;
            _behaviorGraphAgent.Graph = _model.FishItemInstance.FishItemData.FishBehaviorData.BehaviorGraph;
            _behaviorGraphAgent.Init();
            _behaviorGraphAgent.GetVariable("FishZone", out _fishZone);
            _behaviorGraphAgent.GetVariable("HookZone", out _hookZone);
            _behaviorGraphAgent.GetVariable("FishUnitCirclePosition", out _fishUnitCirclePosition);
            _behaviorGraphAgent.GetVariable("HookUnitCirclePosition", out _hookUnitCirclePosition);
            _behaviorGraphAgent.GetVariable("AngleDifference", out _angleDifference);
            _behaviorGraphAgent.GetVariable("FatiguePercent", out _fatiguePercent);
            _behaviorGraphAgent.Restart();
            _behaviorGraphAgent.Start();
        }

        /// <summary>
        /// Update the behavior graph variables with the current state of the fishing board.
        /// </summary>
        private void UpdateBehaviourGraphVariables()
        {
            _fishZone.Value = (BackboardFishZone)(int)_fishingBoardController.FishZone;
            _hookZone.Value = (BackboardFishZone)(int)_fishingBoardController.HookZone;
            _fishUnitCirclePosition.Value = _fishingBoardController.FishUnitCirclePosition;
            _hookUnitCirclePosition.Value = _fishingBoardController.HookUnitCirclePosition;
            _angleDifference.Value = _fishingBoardController.AngleDifference;
            _fatiguePercent.Value = _model.FatigueLevelPercent.CurrentValue;
        }

        /// <summary>
        /// Shutdown the behavior graph when the mini-game ends.
        /// </summary>
        private void ShutdownBehaviorGraph()
        {
            _behaviorGraphAgent.End();
            _behaviorGraphAgent.enabled = false;
        }

        /// <summary>
        /// Reset the fatigue level to half of the maximum.
        /// </summary>
        private void ResetFatigueLevel()
        {
            _model.CurrentFatigueLevel.Value = _fishingBoardConfig.MaxFatigueLevel / 2;
        }

        /// <summary>
        /// Update the fatigue level based on the fishing rod and fish power.
        /// </summary>
        private void UpdateFatigueLevel()
        {
            var fishPower = _model.FishItemInstance.FishItemData.FishBehaviorData.Power;
            var rodPower = _model.FishingRodItemInstance.CurrentPower;
            var fishMultiplier = _fishingBoardController.FishPowerMultiplier;
            var hookMultiplier = _fishingBoardController.HookPowerMultiplier;
            var pullPercent = _fishingBoardController.PullPercent;
            var fatigue = (rodPower * hookMultiplier * pullPercent) - (fishPower * fishMultiplier);
            _model.CurrentFatigueLevel.Value += fatigue * Time.deltaTime;
            _model.CurrentFatigueLevel.Value = Mathf.Clamp(_model.CurrentFatigueLevel.Value, 0, _fishingBoardConfig.MaxFatigueLevel);
            if (_model.CurrentFatigueLevel.Value <= 0)
            {
                LoseFishingBoard();  
            }

            if (_model.CurrentFatigueLevel.Value >= _fishingBoardConfig.MaxFatigueLevel)
            {
                WinFishingBoard();
            }
        }

        /// <summary>
        /// Update the fishing line durability based on the tension from the fish and rod.
        /// </summary>
        private void UpdateFishingLineDurability()
        {
            var currentRod = _model.FishingRodItemInstance;
            var currentFish = _model.FishItemInstance;
            var fishPower = currentFish.FishItemData.FishBehaviorData.Power;
            var rodPower = currentRod.CurrentPower;
            var fishMultiplier = _fishingBoardController.FishPowerMultiplier;
            var hookMultiplier = _fishingBoardController.HookPowerMultiplier;
            var fishingLineTension = (rodPower * hookMultiplier) + (fishPower * fishMultiplier);
            var regenFactor = currentRod.CurrentFishingLineRegenFactor;
            var final = regenFactor - fishingLineTension;
            currentRod.CurrentFishingLineDurability += final * Time.deltaTime;
            currentRod.CurrentFishingLineDurability = Mathf.Clamp(currentRod.CurrentFishingLineDurability,
                0, currentRod.BaseStats.FishingLineDurability);
            PlayTensionSound(_model.FishingLineDurabilityPercent.CurrentValue);
            if (currentRod.CurrentFishingLineDurability <= 0)
            {
                LoseFishingBoard();
            }
        }
        
        /// <summary>
        /// Play the fishing line tension sound based on the durability percentage.
        /// </summary>
        /// <param name="durabilityPercent">The current durability percentage of the fishing line.</param>
        private void PlayTensionSound(float durabilityPercent)
        {
            _fishingLineTensionAudioReference.eventInstance.setParameterByName("Tension", 1 - durabilityPercent);
        }

        /// <summary>
        /// Called when the player loses the fishing board mini-game.
        /// </summary>
        private void LoseFishingBoard()
        {
            Debug.Log("Lose Fishing Board");
            StopFishingBoard();
        }

        /// <summary>
        /// Called when the player wins the fishing board mini-game.
        /// </summary>
        private void WinFishingBoard()
        {
            Debug.Log("Win Fishing Board");
            _fishingBoardController.MoveFishTimeBased(Vector2.zero, 1f);
            PlayTensionSound(_model.FishingLineDurabilityPercent.CurrentValue);
            StopFishingBoard();
        }
        #endregion
    }
}