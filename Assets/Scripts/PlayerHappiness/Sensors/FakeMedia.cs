using System.Text;
using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public class FakeMedia : ISensor
    {
        ICollectorContext m_Context;

        public string name => "fake";

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

        public CustomYieldInstruction Stop()
        {
            m_Context.SetMedia("test", UTF8Encoding.UTF8.GetBytes("THIS IS A TEST DATA!"));
            return null;
        }
    }
}
