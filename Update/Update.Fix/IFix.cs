namespace Update.Fix
{
    public interface IFix
    {
        string Name { get; }
        bool? Check();
        void Apply();
    }
}
