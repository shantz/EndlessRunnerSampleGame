namespace PlayerHappiness
{
    public interface ICollectorContext
    {
        IFrame DoFrame();
        void SetMedia(string name, byte[] data);
	    void SetMetdataFile(string name, string path);
    }
}
