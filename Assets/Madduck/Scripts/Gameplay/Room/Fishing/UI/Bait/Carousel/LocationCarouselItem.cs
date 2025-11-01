using HasanSadikin.Carousel;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LocationCarouselItem : CarouselItem<LocationData>
{
   [SerializeField] private Image _image;
   [SerializeField] private TweenSettings<float> fadeTween;
   [SerializeField] private TweenSettings<float> scalSettings;
   [SerializeField] private TweenSettings<float> deactiveFadeTween;
   [SerializeField] private TweenSettings<float> deacuveScalSettings;
   private Sequence? _sequence;


   protected override void OnDataUpdated(LocationData data)
   {
      base.OnDataUpdated(data);
      _image.sprite = data.sprite;
   }

   protected override void OnActivated()
   {
      base.OnActivated();

      _sequence?.Stop();
      _sequence = Sequence.Create()
         .Group(Tween.Alpha(_image, fadeTween))
         .Group(Tween.Scale(_rectTransform, scalSettings));
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
