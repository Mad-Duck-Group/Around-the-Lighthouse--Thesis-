using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Madduck.Input;
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
        [HideInEditorMode, HideReferenceObjectPicker,
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
            var chunked = instances.Chunk(3).Select(x => x.ToList()).ToList();
            foreach (var chunk in chunked)
            {
                if (chunk.Count <= 0) continue;
                var popUpObject = new FishableItemPopUpObject(chunk);
                var popUp = Create();
                popUp.SetPopUpObject(popUpObject);
                _modalManager.Queue(popUp);
            }
        }
    }
}