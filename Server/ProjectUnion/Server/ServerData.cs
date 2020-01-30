using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectUnion.Server
{
    public class GamePosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Heading { get; set; }

        public float GetHeading()
        {
            return Heading;
        }

        public void SetPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
