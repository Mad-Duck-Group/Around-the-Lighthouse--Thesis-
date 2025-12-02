using System;
using Cysharp.Threading.Tasks;
using HasanSadikin.Carousel;
using Madduck.Input;
using Madduck.Shared;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room.PointingBait
{
    public class PointingBaitView : MonoBehaviour
    {
        [Header("References")]
        [Required,
         SerializeField] private Image rightPointingImage;
        [Required,
         SerializeField] private Image leftPointingImage;
        
        private CarouselController _carouselController;
        private PointingBaitViewModel _viewModel;

        private IDisposable _bindings;
        [Inject]
        public void SetUp(PointingBaitViewModel viewModel,CarouselController carouselController, IPlayerInputHandler playerInputHandler)
        {
            _carouselController = carouselController;
            _viewModel = viewModel;
            Bind();
        }
        
        public void Bind()
        {
            var builder = Disposable.CreateBuilder();

            _viewModel.LeftPressed
                .Where(x => x)
                .SubscribeAwait((b, token) => SetPointingLeft(_carouselController.PointingBaitConfig),AwaitOperation.Drop);

            _viewModel.RightPressed
                .Where(x => x)
                .SubscribeAwait((b, token) => SetPointingRight(_carouselController.PointingBaitConfig),AwaitOperation.Drop);
            _bindings = builder.Build();
        }
        
        
        public async UniTask SetPointingLeft(PointingBaitConfig config)
        {
            var spriteSelected = config.GetLeftSelectedIcon(SelectionIcon.Selected);
            leftPointingImage.sprite = spriteSelected;
            await UniTask.WaitForSeconds(config.iconSwitchDelay / 2);
            var spriteUnSelected = config.GetLeftSelectedIcon(SelectionIcon.Unselected);
            leftPointingImage.sprite = spriteUnSelected;
            await UniTask.WaitForSeconds(config.iconSwitchDelay / 2);
        }

        public async UniTask SetPointingRight(PointingBaitConfig config)
        {
            var spriteSelected = config.GetRightSelectedIcon(SelectionIcon.Selected);
            rightPointingImage.sprite = spriteSelected;
            await UniTask.WaitForSeconds(config.iconSwitchDelay / 2);
            var spriteUnSelected = config.GetRightSelectedIcon(SelectionIcon.Unselected);
            rightPointingImage.sprite = spriteUnSelected;
            await UniTask.WaitForSeconds(config.iconSwitchDelay / 2);
                
        }
        
    }
}
