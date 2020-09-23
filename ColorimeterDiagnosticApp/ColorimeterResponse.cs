using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterResponse
    {
        //public List<string> responseInfo;
        public string FirmwareVersion;
        public string TestFileVersion;
        public string DeviceState;

        public ColorimeterActionType ColorimeterResponseActionType;

        public ColorimeterResponse()
        {
            //responseInfo = new List<String>();
            FirmwareVersion = "";
            TestFileVersion = "";
        }
    }
}
