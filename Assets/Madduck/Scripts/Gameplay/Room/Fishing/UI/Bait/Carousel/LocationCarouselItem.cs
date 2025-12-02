using HasanSadikin.Carousel;
using Madduck.GameData.Bait;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LocationCarouselItem : CarouselItem<BaitItemInstance>
{
   [SerializeField] private Image _image;
   [SerializeField] private TweenSettings<float> fadeTween;
   [SerializeField] private TweenSettings<float> scalSettings;
   [SerializeField] private TweenSettings<float> deactiveFadeTween;
   [SerializeField] private TweenSettings<float> deacuveScalSettings;
   private Sequence? _sequence;


   protected override void OnDataUpdated(BaitItemInstance data)
   {
      base.OnDataUpdated(data);
      _image.sprite = data.ItemData.BaitIcon;
   }

   protected override void OnActivated()
   {
      base.OnActivated();

      _sequence?.Stop();
      _sequence = Sequence.Create()
         .Group(Tween.Alpha(_image, fadeTween))
         .Group(Tween.Scale(_rectTransform, scalSettings));
   }
   public override void SetSelected(bool isSelected)
   {
      var icon = isSelected
         ? Data.ItemData.BaitSelectedIcon
         : Data.ItemData.BaitIcon;

      _image.sprite = icon;
   }
   protected override void OnDeactivated()
   {
      base.OnDeactivated();
      
      _sequence?.Stop();
      _sequence = Sequence.Create()
         .Group(Tween.Alpha(_image, deactiveFadeTween))
         .Group(Tween.Scale(_rectTransform, deacuveScalSettings));
   }
}
