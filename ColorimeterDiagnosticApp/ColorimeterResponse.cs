using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterResponse
    {
        public List<String> responseInfo;
        public String firmwareVersion;
        public String testFileVersion;

        public ColorimeterResponse()
        {
            responseInfo = new List<String>();
            firmwareVersion = "";
            testFileVersion = "";
        }
    }
}
