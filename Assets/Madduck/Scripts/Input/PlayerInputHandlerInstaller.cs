﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Scripts.Input
{
    [Serializable]
    public class PlayerInputHandlerInstaller : IInstaller
    {
        [Title("Input")]
        [SerializeField] private PlayerInputHandler playerInputHandler;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerInputHandler).AsSelf();
        }
    }
}