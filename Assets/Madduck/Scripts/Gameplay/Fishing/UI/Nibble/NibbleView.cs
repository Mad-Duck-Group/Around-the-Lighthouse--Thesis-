using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData.Bait;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class NibbleView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required,
         SerializeField] private Button pullHookButton;
        [Required,
         SerializeField] private Image catchChanceRadial;
        [Required,
         SerializeField] private Image baitIconImage;
        [Required,
         SerializeField] private TMP_Text catchChanceText;

        [Title("Settings")] 
        [SerializeField] private SerializableDictionary<uint, Color> catchStageColors = new();
        
        [Title("Tween")]
        [SerializeField] private TweenSettings catchChanceLerpTweenSettings;
        
        private NibbleCommander _commander;
        private NibbleViewModel _viewModel;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(NibbleCommander commander, NibbleViewModel viewModel)
        {
            _commander = commander;
            _viewModel = viewModel;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.CatchChance
                .Subscribe(OnCatchChanceChanged)
                .AddTo(ref disposableBuilder);
            _viewModel.CatchStage
                .Subscribe(OnCatchStageChanged)
                .AddTo(ref disposableBuilder);
            _viewModel.CurrentBait
                .Subscribe(OnBaitChanged)
                .AddTo(ref disposableBuilder);
            pullHookButton.onClick
                .AsObservable()
                .Subscribe(_ => OnPullHook())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
    
        #region Transitions
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            //await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken); // placeholder for actual transition animation
            SetActive(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            //await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken); // placeholder for actual transition animation
            SetActive(false);
        }
        
        private void CancelTransition()
        {
            // Implement if needed
        }
        #endregion

        private void SetActive(bool active)
        {
            _bindings?.Dispose();
            catchChanceRadial.fillAmount = 0;
            catchChanceRadial.color = catchStageColors[0];
            baitIconImage.enabled = false;
            if (active)
            {
                Bind();
            }

            gameObject.SetActive(active);
        }
        
        private void OnPullHook()
        {
            _commander.PullHookCommand.Execute(Unit.Default);
        }
        
        private void OnCatchChanceChanged(Percentage percentage)
        {
            TweenRadial(percentage);
            catchChanceText.text = percentage.ToPercentageString("F0");
        }
        
        private void OnCatchStageChanged(uint stage)
        {
            if (catchStageColors.TryGetValue(stage, out var color))
            {
                catchChanceRadial.color = color;
            }
        }

        private void OnBaitChanged(BaitItemInstance bait)
        {
            if (bait is null)
            {
                baitIconImage.enabled = false;
            }
            else
            {
                baitIconImage.enabled = true;
                baitIconImage.sprite = bait.ItemData.BaitIcon;
            }
        }

        private void TweenRadial(Percentage percentage)
        {
            Tween.UIFillAmount(catchChanceRadial, percentage.AsFraction, catchChanceLerpTweenSettings);
        }
    }
}