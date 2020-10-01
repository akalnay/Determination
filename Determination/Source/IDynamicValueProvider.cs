namespace Determination
{
    public interface IDynamicValueProvider<T>
    {
        T Value { get; }
    }
}