namespace NetMediaInfoLib
{
    public interface IChild<T>
    {
        T Parent { get; }
    }
}
