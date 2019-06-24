using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public interface IGameSensor
    {
        void SendEvent(string name);
    }

    public class GameSensor : ISensor, IGameSensor
    {
        
        public static IGameSensor instance { get; private set; }
        ICollectorContext m_Context;
        FakeGameSensor m_FakeImpl;
        public string name => "game";
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
            m_FakeImpl = new FakeGameSensor();
            instance = m_FakeImpl;
        }

        public void Start()
        {
            instance = this;
        }

        public CustomYieldInstruction Stop()
        {
            instance = m_FakeImpl;
            return null;
        }

        class FakeGameSensor : IGameSensor {
            public void SendEvent(string name)
            {
                
            }
        }

        public void SendEvent(string name)
        {
            using (var frame = m_Context.DoFrame())
            {
                frame.Write("e", name);
            }
        }
    }
}
