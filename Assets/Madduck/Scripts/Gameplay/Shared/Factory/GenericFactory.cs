using System;

namespace Madduck.Shared
{
    public interface IGenericFactory<out T>
    {
        public T Current { get; }
        public T Create();
    }
}