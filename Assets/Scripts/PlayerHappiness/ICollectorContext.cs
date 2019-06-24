namespace PlayerHappiness
{
    public interface ICollectorContext
    {
        IFrame DoFrame();
        void SetMetadata(string name, byte[] data);

    }
}
