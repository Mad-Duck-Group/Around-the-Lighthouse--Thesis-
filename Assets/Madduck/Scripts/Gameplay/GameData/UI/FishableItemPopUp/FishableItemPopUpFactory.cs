using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class FishableItemPopUpFactory : PopUpFactory<FishableItemPopUpObject>
    {
        [Inject] private readonly IModalManager _modalManager;
        
        [Title("Debug")]
        [HideInEditorMode,
         Button("Test Show")]
        public void TestShow(List<IFishableItemData> itemData)
        {
            var instances = new List<IFishableItemInstance>();
            foreach (var item in itemData)
            {
                IFishableItemInstance instance;
                switch (item)
                {
                    case FishItemData fishItemData:
                        instance = new FishItemInstance(fishItemData, new ModifierSourceMock());
                        instances.Add(instance);
                        break;
                    case ResourceItemData resourceItemData:
                        instance = new ResourceItemInstance(resourceItemData);
                        instances.Add(instance);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(itemData));
                }
            }
            var popUpObject = new FishableItemPopUpObject(instances);
            var popUp = Create();
            popUp.SetPopUpObject(popUpObject);
            _modalManager.Queue(popUp);
        }
    }
}