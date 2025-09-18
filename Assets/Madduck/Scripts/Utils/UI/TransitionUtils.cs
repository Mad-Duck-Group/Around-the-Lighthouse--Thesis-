using System.Threading;
using Cysharp.Threading.Tasks;

namespace Madduck.Utils
{
    public interface ITransitionable
    {
        UniTask TransitionIn(CancellationToken cancellationToken = default);
        UniTask TransitionOut(CancellationToken cancellationToken = default);
    }
    
    public class TransitionMock : ITransitionable
    {
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            await UniTask.CompletedTask;
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            await UniTask.CompletedTask;
        }
    }
}