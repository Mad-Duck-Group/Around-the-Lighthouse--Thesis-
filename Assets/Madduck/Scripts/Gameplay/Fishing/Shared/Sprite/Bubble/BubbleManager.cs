using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Redcode.Extensions;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Shared
{
    public struct BubbleChangedEvent
    {
        public IBubbleView BubbleView { get; private set; }
        public bool IsSpawned { get; private set; }
        
        public BubbleChangedEvent(IBubbleView bubbleView, bool isSpawned)
        {
            BubbleView = bubbleView;
            IsSpawned = isSpawned;
        }
    }
    public class BubbleManager : IDisposable
    {
        private record BubbleSpawnInfo
        {
            public Vector2Int occupiedSegments;
            public PausableTimer bubbleTimer;
        }

        public event Action<BubbleChangedEvent> OnBubbleChanged;
        
        private readonly BubbleManagerConfig _config;
        private readonly PlayerInventory _playerInventory;
        private readonly IAudioManager _audioManager;
        private readonly IBubbleViewFactory _bubbleFactory;
        private readonly ISubscriber<FishingRoomStartedEvent> _fishingRoomStartedEventSubscriber;

        private readonly ObservableDictionary<IBubbleView, BubbleSpawnInfo> _bubbles = new();
        private bool _isPaused;
        private int _currentGuaranteeCount;
        private IDisposable _subscription;
        private IDisposable _bubbleSpawnTimer;
        private AudioReference _bubbleSfx;

        [Inject]
        public BubbleManager(
            BubbleManagerConfig config,
            PlayerInventory playerInventory,
            IAudioManager audioManager,
            IBubbleViewFactory bubbleFactory,
            ISubscriber<FishingRoomStartedEvent> fishingRoomStartedEventSubscriber)
        {
            _config = config;
            _playerInventory = playerInventory;
            _audioManager = audioManager;
            _bubbleFactory = bubbleFactory;
            _fishingRoomStartedEventSubscriber = fishingRoomStartedEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishingRoomStartedEventSubscriber
                .Subscribe(_ =>
                {
                    SpawnBubble();
                    StartSpawnTimer();
                })
                .AddTo(ref disposableBuilder);
            _bubbles.ObserveCountChanged()
                .Prepend(0)
                .Pairwise()
                .Subscribe(x => OnBubbleCountChanged(x.Previous, x.Current))
                .AddTo(ref disposableBuilder);
            _subscription = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _audioManager.StopAudio(_bubbleSfx);
            _subscription?.Dispose();
            _bubbleSpawnTimer?.Dispose();
            foreach (var bubbleInfo in _bubbles.Select(x => x.Value))
            {
                bubbleInfo.bubbleTimer?.Dispose();
            }
        }
        
        private void OnBubbleCountChanged(int oldCount, int newCount)
        {
            DebugUtils.Log("[BubbleManager] Bubble count changed from " + oldCount + " to " + newCount);
            if (newCount > 0)
            {
                if (!_bubbleSfx.IsPlaying()) 
                    _bubbleSfx = _audioManager.PlayAudio(_config.BubbleSfx, Vector3.zero);
            }
            else
            {
                _audioManager.StopAudio(_bubbleSfx);
            }
            if (newCount >= _config.BubbleMaxLimits)
            {
                _bubbleSpawnTimer?.Dispose();
                _bubbleSpawnTimer = null;
            }
            else if (oldCount == _config.BubbleMaxLimits && newCount < _config.BubbleMaxLimits)
            {
                // Still under limit, ensure timer is running
                if (_bubbleSpawnTimer == null)
                {
                    StartSpawnTimer();
                }
            }
        }

        private void StartSpawnTimer()
        {
            _bubbleSpawnTimer = Observable.Timer(TimeSpan.FromSeconds(_config.BubbleSpawnInterval))
                .Subscribe(_ =>
                {
                    StartSpawnTimer();
                    SpawnBubble();
                });
        }

        private void SpawnBubble()
        {
            if (_currentGuaranteeCount >= _config.BubbleGuaranteeCount)
            {
                var chance = _playerInventory.CurrentFishingRod.CurrentStats.CurrentBubbleSpawnChance;
                if (!Percentage.TryRoll(chance))
                {
                    DebugUtils.Log($"[BubbleManager] Bubble spawn chance roll failed. Chance: {chance}");
                    return;
                }
            }
            else
            {
                _currentGuaranteeCount++;
            }
            var prototype = _bubbleFactory.Prototype;
            var count = LengthToSegmentCount(prototype.BubbleLength);
            if (!TryFindRandomAvailableSegments(count, out var randomSegments))
            {
                DebugUtils.LogWarning("[BubbleManager] No available segments found for bubble spawn.");
                return;
            }
            var bubble = _bubbleFactory.Create();
            var position = SegmentToPosition(randomSegments);
            var center = Mathf.Lerp(position.x, position.y, 0.5f);
            var spawnPosition = new Vector2(center, _config.BubbleYOffset);
            bubble.SetUp(spawnPosition, BubbleType.Standard);
            var timer = new PausableTimer(TimeSpan.FromSeconds(_config.BubbleStayDuration),
                () => DespawnBubble(bubble));
            if (_isPaused) timer.Pause();
            var bubbleInfo = new BubbleSpawnInfo
            {
                occupiedSegments = randomSegments,
                bubbleTimer = timer
            };
            _bubbles.Add(bubble, bubbleInfo);
            OnBubbleChanged?.Invoke(new BubbleChangedEvent(bubble, isSpawned: true));
            bubble.TransitionIn().Forget();
        }
        
        private void DespawnBubble(IBubbleView bubble)
        {
            if (!_bubbles.TryGetValue(bubble, out var bubbleInfo))
            {
                DebugUtils.LogError("[BubbleManager] Attempted to despawn a bubble that is not managed.");
                return;
            }
            bubbleInfo.bubbleTimer?.Dispose();
            bubble.TransitionOut()
                .ContinueWith(() =>
                {
                    _bubbles.Remove(bubble);
                    OnBubbleChanged?.Invoke(new BubbleChangedEvent(bubble, isSpawned: false));
                });
        }
        
        public void PauseAllBubbles()
        {
            _isPaused = true;
            foreach (var bubbleInfo in _bubbles.Select(x => x.Value))
            {
                bubbleInfo.bubbleTimer?.Pause();
            }
        }
        
        public void ResumeAllBubbles()
        {
            _isPaused = false;
            foreach (var bubbleInfo in _bubbles.Select(x => x.Value))
            {
                bubbleInfo.bubbleTimer?.Start();
            }
        }

        public bool TryLandOnBubble(Vector2 position, out IBubbleView bubble)
        {
            bubble = null;
            foreach (var bubbleView in _bubbles.Select(x => x.Key))
            {
                if (!TryLandOnBubble(position, bubbleView)) continue;
                bubble = bubbleView;
                return true;
            }
            DebugUtils.Log("[BubbleManager] No bubble landed on.");
            return false;
        }

        public bool TryLandOnBubble(Vector2 position, IBubbleView bubble)
        {
            var bubbleInfo = _bubbles.GetValueOrDefault(bubble);
            if (bubbleInfo == null) return false;
            var bound = SegmentToPosition(bubbleInfo.occupiedSegments);
            var leftBound = bound.x;
            var rightBound = bound.y;
            DebugUtils.Log("[BubbleManager] Checking bubble at bounds: " + leftBound + " to " + rightBound);
            if (position.x >= leftBound && position.x <= rightBound)
            {
                DebugUtils.Log($"[BubbleManager] Landed on bubble. Type: {bubble.BubbleType}");
                return true;
            }
            return false;
        }

        #region Utils
        private int GetSegmentCount()
        {
            return Mathf.FloorToInt((_config.BubbleSpawnRange.y - _config.BubbleSpawnRange.x) * _config.RangeSubdivision) + 1;
        }

        private Vector2 SegmentToPosition(Vector2Int segment)
        {
            return new Vector2(
                SegmentToPosition(segment.x),
                SegmentToPosition(segment.y)
            );
        }
        
        private float SegmentToPosition(int segmentIndex)
        {
            var segmentCount = GetSegmentCount();
            var start = _config.BubbleSpawnRange.x;
            var end = _config.BubbleSpawnRange.y;
            var segmentWidth = (end - start) / (segmentCount - 1);
            return start + segmentIndex * segmentWidth;
        }
        
        private int LengthToSegmentCount(float length)
        {
            var segmentCount = GetSegmentCount();
            var start = _config.BubbleSpawnRange.x;
            var end = _config.BubbleSpawnRange.y;
            var segmentWidth = (end - start) / (segmentCount - 1);
            return Mathf.CeilToInt(length / segmentWidth); //Ceil to ensure we cover the length
        }
        
        private bool TryFindRandomAvailableSegments(int requiredSegmentCount, out Vector2Int randomSegments)
        {
            var segmentCount = GetSegmentCount();
            var occupied = new bool[segmentCount];
            randomSegments = new Vector2Int(-1, -1);
    
            // Mark occupied segments
            foreach (var bubbleInfo in _bubbles.Select(x => x.Value))
            {
                for (int i = bubbleInfo.occupiedSegments.x; i <= bubbleInfo.occupiedSegments.y; i++)
                {
                    if (i >= 0 && i < segmentCount)
                        occupied[i] = true;
                }
            }
    
            // Find all available contiguous segments
            var availableRanges = new List<Vector2Int>();
    
            for (int i = 0; i < segmentCount; i++)
            {
                if (occupied[i]) continue;
                int start = i;
                // Find the end of this contiguous free block
                while (i < segmentCount && !occupied[i])
                {
                    i++;
                }
                int end = i - 1;
                int length = end - start + 1;
            
                if (length >= requiredSegmentCount)
                {
                    availableRanges.Add(new Vector2Int(start, end));
                }
            }
    
            if (availableRanges.Count == 0)
                return false;
    
            // Pick a random available range that can fit our required segments
            var randomRange = availableRanges.GetRandomElement();
            int maxStart = randomRange.y - requiredSegmentCount + 1;
    
            // Randomly choose a starting position within this range
            int randomStart = UnityEngine.Random.Range(randomRange.x, maxStart + 1);
            randomSegments = new Vector2Int(randomStart, randomStart + requiredSegmentCount - 1);
    
            return true;
        }
        #endregion
    }
}