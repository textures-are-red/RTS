public interface IStackable
{
    int MaxStack { get; }
    int CurrentCount { get; }

    bool TryStackTo(int countOfNewResurces);
    bool TryUnstackFrom(int countToUnstack);
}
