namespace Madduck.Utils
{
    public interface IFactory<out T>
    {
        public T Current { get; }
        public T Create();
    }
}