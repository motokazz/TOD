public interface ICondition
{
    bool IsMet(FloorManager manager);
    void Reset();
}