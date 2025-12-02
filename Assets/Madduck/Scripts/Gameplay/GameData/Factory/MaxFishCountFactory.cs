using System;
using Madduck.Shared;
using Madduck.Utils;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public interface IMaxFishCountProvider
    {
        uint GetMaxFishCount();
    }
    public class MaxFishCountFactory : IFactory<uint>
    {
        private readonly IMaxFishCountProvider _dayManager;
        
        public uint Current { get; private set; }
        
        [Inject]
        public MaxFishCountFactory(IMaxFishCountProvider dayManager)
        {
            _dayManager = dayManager;
        }

        public uint Create()
        {
            Current = _dayManager.GetMaxFishCount();
            return Current;
        }
    }
    
    [Serializable]
    public class MaxFishCountFactoryMock : IFactory<uint>
    {
        [SerializeField] private uint fixedMaxFishCount;
        
        public uint Current => fixedMaxFishCount;
        public MaxFishCountFactoryMock(){} // For inspector serialization
        public MaxFishCountFactoryMock(uint fixedMaxFishCount)
        {
            this.fixedMaxFishCount = fixedMaxFishCount;
        }
        
        public uint Create() => fixedMaxFishCount;
    }
}