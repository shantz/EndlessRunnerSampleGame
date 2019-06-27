using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public interface IGameSensor
    {
        void SendEvent(string name);
        void SendEvent(string name, float value);
    }

    public class GameSensor : ISensor, IGameSensor
    {
        
        public static IGameSensor instance { get; private set; }
        ICollectorContext m_Context;
        FakeGameSensor m_FakeImpl;
        public string name => "game";
        public bool useFrames => true;
        public int[] projectedValues => new[] { /* f */ 1,  /* i */ 0,  /* s */ 1,  /* v2 */ 0,  /* v3 */ 0,  /* q */ 0 };
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

            public void SendEvent(string name, float value)
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

        public void SendEvent(string name, float value)
        {
            using (var frame = m_Context.DoFrame())
            {
                frame.Write("e", name);
                frame.Write("v", value);
            }
        }
    }
}
