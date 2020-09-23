using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{

    //this class represents a data structure for an individual test result

    class TestResultRecord
    {
        public float TestResult;
        public string TestName;
        public string TestNameAltLanguage1;


        public TestResultRecord(byte[] incomingArray)
        {
            List<byte> incomingArrayAsList = incomingArray.ToList<byte>();

            //var incomingArraySlice = incomingArrayAsList.GetRange(0, 4).ToArray();

            TestResult = System.BitConverter.ToSingle(incomingArrayAsList.GetRange(0, 4).ToArray(), 0);
            //TestName = incomingArrayAsList.GetRange(4, 26).ToArray()
        }

    }
}
