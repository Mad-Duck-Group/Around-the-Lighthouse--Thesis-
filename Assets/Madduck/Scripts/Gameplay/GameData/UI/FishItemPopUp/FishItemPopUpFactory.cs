using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class FishItemPopUpFactory : PopUpFactory<FishItemPopUpObject>
    {
        [Title("Debug")]
        [HideInEditorMode,
         Button("Test Show")]
        public void TestShow(FishItemData fishItemData)
        {
            var instance = new FishItemInstance(fishItemData);
            var popUpObject = new FishItemPopUpObject(instance);
            var popUp = Create();
            popUp.SetPopUpObject(popUpObject);
            popUp.Show().Forget();
        }
    }
}