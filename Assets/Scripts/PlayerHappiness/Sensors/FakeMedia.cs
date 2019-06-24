using System.Text;

namespace PlayerHappiness.Sensors
{
    public class FakeMedia : ISensor
    {
        ICollectorContext m_Context;
        
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        public void Start()
        {
            using(var frame = m_Context.DoFrame())
            {
                frame.Write("test", 0);
            }
        }

        public void Stop()
        {
            m_Context.SetMedia("test", UTF8Encoding.UTF8.GetBytes("THIS IS A TEST DATA!"));
        }
    }
}
