using System.Threading;
using Cysharp.Threading.Tasks;
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
        
        public Button Button => button;
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            button.gameObject.SetActive(true);
            buttonText.gameObject.SetActive(false);
            animator.enabled = true;
            animator.Play("In");
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, cancellationToken: cancellationToken);
            animator.Play("Empty");
            animator.enabled = false;
            buttonText.gameObject.SetActive(true);
            button.image.sprite = initialSprite;
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            animator.enabled = true;
            animator.Play("Out");
            buttonText.gameObject.SetActive(false);
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, cancellationToken: cancellationToken);
            animator.Play("Empty");
            animator.enabled = false;
            button.gameObject.SetActive(false);
        }
    }
}