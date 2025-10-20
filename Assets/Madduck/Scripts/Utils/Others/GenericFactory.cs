namespace Madduck.Utils
{
    public interface IGenericFactory<out T>
    {
        public T Current { get; }
        public T Create();
    }
}