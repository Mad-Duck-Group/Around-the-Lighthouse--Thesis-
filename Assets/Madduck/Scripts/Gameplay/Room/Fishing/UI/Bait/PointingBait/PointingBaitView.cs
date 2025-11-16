using System;
using HasanSadikin.Carousel;
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
        
        private IDisposable _bindings;
        [Inject]
        public void SetUp(CarouselController carouselController)
        {
            _carouselController = carouselController;
            Bind();
        }

        public void Bind()
        {
            var builder = Disposable.CreateBuilder();

            _carouselController.OnPointingStateChanged
                .Subscribe(selectionIcon =>
                {
                    SetPointing(_carouselController.PointingBaitConfig, selectionIcon);
                })
                .AddTo(ref builder);

            _bindings = builder.Build();
        }
        

        public void SetPointing(PointingBaitConfig config, SelectionIcon selectionIcon)
        {
            if (config.pointintRightBaitIconSprites.TryGetValue(selectionIcon, out var rightSprite))
            {
                rightPointingImage.sprite = rightSprite;
            }

            if (config.pointintLeftBaitIconSprites.TryGetValue(selectionIcon, out var leftSprite))
            {
                leftPointingImage.sprite = leftSprite;
            }
        }
        
    }
}
