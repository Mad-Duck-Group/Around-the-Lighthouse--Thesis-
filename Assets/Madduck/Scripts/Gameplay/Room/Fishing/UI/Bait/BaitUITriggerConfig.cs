using System;
using Madduck.Shared;
using Madduck.Utils;
using UnityEngine;

namespace Madduck.Room
{
    [Serializable]
    public class BaitUITriggerConfig
    {
        public UITriggerType uiTriggerType;
        public GameObject Value { get; }
    }
}
