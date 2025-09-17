using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

public class RoomPreset : MonoBehaviour
{
    #region Inspactor

    [Title("References"),BoxGroup("References"),SerializeField] 
    public SpriteRenderer skyRenderer;
    [BoxGroup("References"),SerializeField] 
    public SpriteRenderer[] rockRenderers;
    [BoxGroup("References"),SerializeField] 
    public SpriteRenderer[] waveRenderers;
    
    
    [Title("Variants"),BoxGroup("Variants"),SerializeField] 
    private Sprite[] skyVariants;
    [BoxGroup("Variants"),SerializeField]
    private Sprite[] rockVariants;
    [BoxGroup("Variants"),SerializeField]
    private Sprite[] waveVariants;
    
    [Title("Tween Settings"),BoxGroup("Tween Settings"),SerializeField]
    public ShakeSettings rockShakeSettings;
    [BoxGroup("Tween Settings"),SerializeField]
    public TweenSettings<float> waveTweenSettings;
    #endregion

    #region Set Room

    public void ApplySprites()
    {
        if (skyRenderer != null && skyVariants.Length > 0)
            skyRenderer.sprite = skyVariants[Random.Range(0, skyVariants.Length)];
        
        if (rockRenderers != null && rockVariants.Length > 0)
        {
            foreach (var render in rockRenderers)
            {
                if (render != null)
                    render.sprite = rockVariants[Random.Range(0, rockVariants.Length)];
            }
        }
        
        if (waveRenderers != null && waveVariants.Length > 0)
        {
            foreach (var render in waveRenderers)
            {
                if (render != null)
                    render.sprite = waveVariants[Random.Range(0, waveVariants.Length)];
            }
        }
        //ShakeRock();
        AnimateWave();
    }
    #endregion

    #region Tween

    
    // public void ShakeRock(int index = -1)
    // {
    //         if (rockRenderers == null || rockRenderers.Length == 0) return;
    //
    //         if (index >= 0 && index < rockRenderers.Length)
    //         {
    //            
    //             Tween.ShakeLocalPosition(rockRenderers[0].transform ,rockShakeSettings);
    //         }
    //         else
    //         {
    //             
    //             foreach (var rock in rockRenderers)
    //             {
    //                 if (rock != null)
    //                     Tween.ShakeLocalPosition(rock.transform ,rockShakeSettings);
    //                     
    //             }
    //         }
    // }


        public void AnimateWave(int index = -1)
        {
            if (waveRenderers == null || waveRenderers.Length == 0) return;

            if (index >= 0 && index < waveRenderers.Length)
            {
               
                var wave = waveRenderers[index].transform;
                Tween.LocalPositionY(wave, waveTweenSettings);
                
            }
            else
            {
               
                foreach (var waveRenderer in waveRenderers)
                {
                    if (waveRenderer != null)   
                    {
                        var wave = waveRenderer.transform;
                        float startY = wave.localPosition.y; 
                        float offset = waveTweenSettings.endValue; 
                        Tween.LocalPositionY(wave,
                            startValue: startY,
                            endValue: startY + offset ,
                            waveTweenSettings.settings.duration,
                            waveTweenSettings.settings.ease,
                            cycles: waveTweenSettings.settings.cycles,
                            cycleMode: CycleMode.Yoyo,
                            startDelay: Random.Range(0f, waveTweenSettings.settings.startDelay));
                    }
                }
            }
        }
        
    

    #endregion

}
