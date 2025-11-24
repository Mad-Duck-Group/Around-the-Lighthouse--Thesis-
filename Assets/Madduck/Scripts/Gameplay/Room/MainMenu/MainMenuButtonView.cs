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
        
        private Sprite _initialSprite;
        
        public Button Button => button;

        private void Awake()
        {
            _initialSprite = button.image.sprite;
        }
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            button.gameObject.SetActive(true);
            buttonText.gameObject.SetActive(false);
            animator.Play("In");
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, cancellationToken: cancellationToken);
            animator.Play("Empty");
            buttonText.gameObject.SetActive(true);
            button.image.sprite = _initialSprite;
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            animator.Play("Out");
            animator.speed = 1;
            buttonText.gameObject.SetActive(false);
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, cancellationToken: cancellationToken);
            animator.Play("Empty");
            button.gameObject.SetActive(false);
        }
    }
}