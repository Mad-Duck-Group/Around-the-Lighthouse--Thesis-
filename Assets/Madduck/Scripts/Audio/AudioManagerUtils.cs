using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Madduck.Audio
{
    #region Data Structures
    public interface IAudioIdentifier
    {
        public Type GetIdentifierType();
        public bool TryGetIdentifier<TId>(out TId identifier);
    }

    public interface IAudioIdentifier<T> : IAudioIdentifier
    {
        public T Identifier { get; set; }
    }
    
    public record AudioIdentifier<T> : IAudioIdentifier<T>
    {
        public T Identifier { get; set; }
        
        public AudioIdentifier(T identifier)
        {
            Identifier = identifier;
        }
        
        public Type GetIdentifierType()
        {
            return typeof(T);
        }
        
        public bool TryGetIdentifier<TId>(out TId identifier)
        {
            if (typeof(TId) == GetIdentifierType())
            {
                identifier = (TId)(object)Identifier;
                return true;
            }
            identifier = default;
            return false;
        }
    }
    
    public record AudioReference
    {
        public EventInstance eventInstance;
        public readonly IAudioIdentifier identifier;
        
        public AudioReference(EventInstance eventInstance, IAudioIdentifier identifier = null)
        {
            this.eventInstance = eventInstance;
            this.identifier = identifier;
        }
    }
    #endregion
    
    #region Interfaces
    public interface IAudioManager
    {
        AudioReference PlayAudio(EventReference eventReference, Vector3 position, IAudioIdentifier id = null, Transform parent = null);
        void PlayAudioOneShot(EventReference eventReference, Vector3 position);
        void SetPauseAudio(AudioReference audioReference, bool pause);
        void SetPauseAllAudioInIdentifier(IAudioIdentifier id, bool pause);
        void SetPauseAllIndexedAudio(bool pause);
        void SetPauseAllWildAudio(bool pause);
        void SetPauseAllAudio(bool pause);
        void StopAudio(AudioReference audioReference, STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT);
        void StopAllAudioInIdentifier(IAudioIdentifier id);
        void StopAllIndexedAudio();
        void StopAllWildAudio();
        void StopAllAudio();
        bool TryFindAudioReference(IAudioIdentifier id, out AudioReference audioReference);
    }

    public interface IAudioBusManager
    {
        bool GetBusData(BusType busType, out BusData busData);
        bool GetBusMuteState(BusType busType, out bool isMuted);
        bool GetBusVolume(BusType busType, out float volume, VolumeUnit outUnit);
        void SetMuteBus(BusType busType, bool mute);
        void ToggleMuteBus(BusType busType);
        void SetVolumeBus(BusType busType, float value, VolumeUnit inUnit);
        void StopAllAudioInBus(BusType busType, STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT);
    }
    #endregion
    
    #region Mocks
    public class AudioManagerMock : IAudioManager
    {
        public AudioReference PlayAudio(EventReference eventReference, Vector3 position, IAudioIdentifier id = null, Transform parent = null)
        {
            return new AudioReference(new EventInstance());
        }
        public void PlayAudioOneShot(EventReference eventReference, Vector3 position){ }
        public void SetPauseAudio(AudioReference audioReference, bool pause){ }
        public void SetPauseAllAudioInIdentifier(IAudioIdentifier id, bool pause){ }
        public void SetPauseAllIndexedAudio(bool pause){ }
        public void SetPauseAllWildAudio(bool pause){ }
        public void SetPauseAllAudio(bool pause){ }
        public void StopAudio(AudioReference audioReference, STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT){ }
        public void StopAllAudioInIdentifier(IAudioIdentifier id){ }
        public void StopAllIndexedAudio(){ }
        public void StopAllWildAudio(){ }
        public void StopAllAudio(){ }
        public bool TryFindAudioReference(IAudioIdentifier id, out AudioReference audioReference)
        {
            audioReference = null;
            return false;
        }
    }
    
    public class AudioBusManagerMock : IAudioBusManager
    {
        public bool GetBusData(BusType busType, out BusData busData)
        {
            busData = null;
            return false;
        }
        public bool GetBusMuteState(BusType busType, out bool isMuted)
        {
            isMuted = false;
            return false;
        }
        public bool GetBusVolume(BusType busType, out float volume, VolumeUnit outUnit)
        {
            volume = 0f;
            return false;
        }
        public void SetMuteBus(BusType busType, bool mute){ }
        public void ToggleMuteBus(BusType busType){ }
        public void SetVolumeBus(BusType busType, float value, VolumeUnit inUnit){ }
        public void StopAllAudioInBus(BusType busType, STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT){ }
    }
    #endregion
}