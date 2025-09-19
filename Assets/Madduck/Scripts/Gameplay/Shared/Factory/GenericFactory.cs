namespace Madduck.Shared
{
    public interface IGenericFactory<out T>
    {
        public T Create();
    }
}