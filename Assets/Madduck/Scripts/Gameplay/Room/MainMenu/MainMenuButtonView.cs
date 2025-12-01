using System.Threading;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Madduck.Audio;
using Madduck.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class MainMenuButtonView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required, 
         SerializeField] private Button button;
        [Required, 
         SerializeField] private Animator animator;
        [Required, 
         SerializeField] private TMP_Text buttonText;
        [Required, 
         SerializeField] private Sprite initialSprite;
        
        [Title("Audio")]
        [Required, 
         SerializeField] private EventReference buttonClickSound;
        
        public Button Button => button;
        
        private IAudioManager _audioManager;
        
        public void SetUp(IAudioManager audioManager)
        {
            _audioManager = audioManager;
            button.gameObject.SetActive(false);
            buttonText.gameObject.SetActive(false);
        }
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            button.gameObject.SetActive(true);
            buttonText.gameObject.SetActive(false);
            animator.enabled = true;
            animator.Play("In");
            _audioManager.PlayAudioOneShot(buttonClickSound, Vector3.zero);
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, cancellationToken: cancellationToken);
            animator.enabled = false;
            buttonText.gameObject.SetActive(true);
            button.image.sprite = initialSprite;
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            animator.enabled = true;
            animator.Play("Out");
            buttonText.gameObject.SetActive(false);
            _audioManager.PlayAudioOneShot(buttonClickSound, Vector3.zero);
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, cancellationToken: cancellationToken);
            animator.enabled = false;
            button.gameObject.SetActive(false);
        }
    }
}