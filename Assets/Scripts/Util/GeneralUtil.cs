namespace Reactics.Util
{
    public interface IMutableExchangeable<T>
    {
        T AsMutable();

        T AsImmutable();
    }

}