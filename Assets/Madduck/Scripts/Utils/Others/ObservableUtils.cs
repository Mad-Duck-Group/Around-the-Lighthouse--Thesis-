using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace Madduck.Scripts.Utils.Others
{
    public static class ObservableUtils
    {
        /// <summary>
        /// Starts updating every frame when the predicate becomes true, stops when it becomes false
        /// </summary>
        /// <typeparam name="T">The type of the source observable</typeparam>
        /// <param name="source">The source observable to watch</param>
        /// <param name="predicate">The condition that determines when to start updating</param>
        /// <param name="frameProvider">Optional frame provider for custom update timing</param>
        /// <returns>An observable that emits every frame while the predicate is true</returns>
        public static Observable<Unit> EveryUpdateWhen<T>(this Observable<T> source, Func<T, bool> predicate, FrameProvider frameProvider = null)
        {
            var ticker = frameProvider == null 
                ? Observable.EveryUpdate() 
                : Observable.EveryUpdate(frameProvider);
    
            return source
                .SelectMany(t => predicate(t) 
                    ? ticker.TakeUntil(source.Where(x => !predicate(x))) 
                    : Observable.Empty<Unit>());
        }

        /// <summary>
        /// Ignores the first value emitted when subscribing to the observable. Useful for input handling to avoid immediate triggers.
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> IgnoreFirstValueWhenSubscribe<T>(this Observable<T> source)
        {
            return source.Skip(1);
        }
    }
}