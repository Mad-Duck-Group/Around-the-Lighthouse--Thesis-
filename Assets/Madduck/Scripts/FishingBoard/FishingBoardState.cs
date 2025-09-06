using System;
using Madduck.Scripts.Audio;
using Madduck.Scripts.FishingBoard.UI;
using Madduck.Scripts.FishingBoard.UI.Model;
using R3;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using VContainer.Unity;

namespace Madduck.Scripts.FishingBoard
{
    [Serializable]
    public class FishingBoardState : IStartable, IDisposable
    {
        #region Inspector
        [Title("Debug")] 
        [DisplayAsString]
        [ShowInInspector] private float _currentFatigueLevel;
        [Button("Test Start")]
        public void TestStart() => StartFishingBoard();
        #endregion

        #region Fields & Properties
        private readonly FishingBoardConfig _fishingBoardConfig;
        private readonly FishingBoardController _fishingBoardController;
        private readonly BehaviorGraphAgent _behaviorGraphAgent;
        private readonly AudioManager _audioManager;
        private readonly FishingBoardModel _model;
        private readonly FishingBoardMinigameReference _fishingBoardMinigameReference;
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
            FishingBoardModel model,
            FishingBoardMinigameReference fishingBoardMinigameReferences)
        {
            _fishingBoardConfig = fishingBoardConfig;
            _fishingBoardController = fishingBoardController;
            _behaviorGraphAgent = behaviorGraphAgent;
            _audioManager = audioManager;
            _model = model;
            _fishingBoardMinigameReference = fishingBoardMinigameReferences;
        }
        #endregion

        #region Life Cycle
        public void Start()
        {
            _fishingBoardController.SetActive(false);
        }

        public void Dispose()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;
        }

        private void StartFishingBoard()
        {
            if (_updateSubscription != null) return;
            _fishingBoardController.SetActive(true);
            _fishingBoardMinigameReference.Initialize();
            ResetFatigueLevel();
            InitializeBehaviorGraph();
            _fishingLineTensionAudioReference = _audioManager.PlayAudio(_fishingBoardConfig.FishingLineTensionSfx, Vector3.zero);
            _updateSubscription = Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => Update());
        }

        public void Update()
        {
            UpdateFatigueLevel();
            UpdateFishingLineDurability();
            UpdateBehaviourGraphVariables();
        }

        public void StopFishingBoard()
        {
            _updateSubscription?.Dispose();
            _updateSubscription = null;
            ShutdownBehaviorGraph();
            _behaviorGraphAgent.enabled = false;
            _currentFatigueLevel = _fishingBoardConfig.MaxFatigueLevel / 2;
            _audioManager.StopAudio(_fishingLineTensionAudioReference);
            _fishingBoardController.SetActive(false);
        }
        #endregion

        #region Fishing Board

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

        private void UpdateBehaviourGraphVariables()
        {
            _fishZone.Value = (BackboardFishZone)(int)_fishingBoardController.FishZone;
            _hookZone.Value = (BackboardFishZone)(int)_fishingBoardController.HookZone;
            _fishUnitCirclePosition.Value = _fishingBoardController.FishUnitCirclePosition;
            _hookUnitCirclePosition.Value = _fishingBoardController.HookUnitCirclePosition;
            _angleDifference.Value = _fishingBoardController.AngleDifference;
            _fatiguePercent.Value = _currentFatigueLevel / _fishingBoardConfig.MaxFatigueLevel;
        }

        private void ShutdownBehaviorGraph()
        {
            _behaviorGraphAgent.End();
            _behaviorGraphAgent.enabled = false;
        }

        private void ResetFatigueLevel()
        {
            _currentFatigueLevel = _fishingBoardConfig.MaxFatigueLevel / 2;
        }

        private void UpdateFatigueLevel()
        {
            var fishPower = _model.FishItemInstance.FishItemData.FishBehaviorData.Power;
            var rodPower = _model.FishingRodItemInstance.CurrentPower;
            var fishMultiplier = _fishingBoardController.FishPowerMultiplier;
            var hookMultiplier = _fishingBoardController.HookPowerMultiplier;
            var pullPercent = _fishingBoardController.PullPercent;
            var fatigue = (rodPower * hookMultiplier * pullPercent) - (fishPower * fishMultiplier);
            _currentFatigueLevel += fatigue * Time.deltaTime;
            _currentFatigueLevel = Mathf.Clamp(_currentFatigueLevel, 0, _fishingBoardConfig.MaxFatigueLevel);
            _fishingBoardMinigameReference.SetFatigue(_currentFatigueLevel / _fishingBoardConfig.MaxFatigueLevel);
            if (_currentFatigueLevel <= 0)
            {
                LoseFishingBoard();  
            }

            if (_currentFatigueLevel >= _fishingBoardConfig.MaxFatigueLevel)
            {
                WinFishingBoard();
            }
        }

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
            var currentPercentDurability = currentRod.CurrentFishingLineDurability /
                                           currentRod.BaseStats.FishingLineDurability;
            PlayTensionSound(currentPercentDurability);
            var shakePercent = 1 - currentPercentDurability;
            _fishingBoardMinigameReference.ShakeReelingSlider(shakePercent);
            if (currentRod.CurrentFishingLineDurability <= 0)
            {
                LoseFishingBoard();
            }
        }


        private void PlayTensionSound(float currentPercentDurability)
        {
            _fishingLineTensionAudioReference.eventInstance.setParameterByName("Tension", 1 - currentPercentDurability);
        }

        private void LoseFishingBoard()
        {
            Debug.Log("Lose Fishing Board");
            StopFishingBoard();
        }

        private void WinFishingBoard()
        {
            Debug.Log("Win Fishing Board");
            _fishingBoardController.MoveFishTimeBased(Vector2.zero, 1f);
            var currentFishingRod = _model.FishingRodItemInstance;
            currentFishingRod.CurrentFishingLineDurability = currentFishingRod.BaseStats.FishingLineDurability;
            var currentPercentDurability = currentFishingRod.CurrentFishingLineDurability /
                                           currentFishingRod.BaseStats.FishingLineDurability;
            PlayTensionSound(currentPercentDurability);
            var shakePercent = 1 - currentPercentDurability;
            _fishingBoardMinigameReference.ShakeReelingSlider(shakePercent);
            StopFishingBoard();
        }

        #endregion
    }
}