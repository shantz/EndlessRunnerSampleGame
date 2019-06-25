namespace PlayerHappiness
{
    public interface ICollectorContext
    {
        IFrame DoFrame();
        void SetMetdataFile(string name, string path);
    }
}
