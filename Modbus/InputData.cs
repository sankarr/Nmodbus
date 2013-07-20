using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus
{
    class InputData
    {

        //  Pressure (bar)	  Temperature (C)	 DP (bar)	  Velocity (m/sec)	Quality factor	Water Cut	Scaled Density

        public float Pressure { set; get; }
        public float Temperature { set; get; }
        public float DP { set; get; }
        public float Velocity { set; get; }
        public float Quality_factor { set; get; }
        public float Water_Cut { set; get; }
        public ushort Scaled_Density { set; get; }
    }
}
