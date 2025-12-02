using System;
using Madduck.Shared;
using Madduck.Utils;
using UnityEngine;

namespace Madduck.Room
{
    [Serializable]
    public struct BaitUITriggerConfig
    {
        public GameObject before;
        public GameObject after;
        public CanvasGroup baitUICanvasGroup;
    }
}
