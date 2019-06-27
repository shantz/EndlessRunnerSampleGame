using System;

namespace PlayerHappiness {
    struct FastFrameInfo
    {
        public float timestamp;
        public int startFloats;
        public int lengthFloats;
        
        public int startInts;
        public int lengthInts;
        
        public int startVector2s;
        public int lengthVector2s;
        
        public int startVector3s;
        public int lengthVector3s;
        
        public int startQuaternions;
        public int lengthQuaternions;
        
        public int startStrings;
        public int lengthStrings;

        public FastFrameInfo(FastCollectorContext context, float timestamp)
        {
            this.timestamp = timestamp;
            
            startFloats = context.floats.Count;
            startInts = context.ints.Count;
            startVector2s = context.vector2s.Count;
            startVector3s = context.vector3s.Count;
            startQuaternions = context.quaternions.Count;
            startStrings = context.strings.Count;
            lengthFloats = 0;
            lengthInts = 0;
            lengthVector2s = 0;
            lengthVector3s = 0;
            lengthQuaternions = 0;
            lengthStrings = 0;
        }
    }
}