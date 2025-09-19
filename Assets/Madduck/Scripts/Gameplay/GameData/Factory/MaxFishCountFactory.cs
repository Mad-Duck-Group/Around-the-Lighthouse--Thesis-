using System;
using Madduck.Shared;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public interface IMaxFishCountProvider
    {
        uint GetMaxFishCount();
    }
    public class MaxFishCountFactory : IGenericFactory<uint>
    {
        private readonly IMaxFishCountProvider _dayManager;
        
        [Inject]
        public MaxFishCountFactory(IMaxFishCountProvider dayManager)
        {
            _dayManager = dayManager;
        }
        
        public uint Create()
        {
            return _dayManager.GetMaxFishCount();
        }
    }
    
    [Serializable]
    public class MaxFishCountFactoryMock : IGenericFactory<uint>
    {
        [SerializeField] private uint fixedMaxFishCount;
        
        public MaxFishCountFactoryMock(){} // For inspector serialization
        public MaxFishCountFactoryMock(uint fixedMaxFishCount)
        {
            this.fixedMaxFishCount = fixedMaxFishCount;
        }
        public uint Create() => fixedMaxFishCount;
    }
}