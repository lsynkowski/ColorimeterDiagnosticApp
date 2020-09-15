using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterRequest
    {

        public ColorimeterRequestType ColorimeterRequestType;
        //public Boolean transferRequest;
        //public Boolean testFileVersionRequest;
        //public Boolean firmwareVersionRequest;
        //public Boolean deviceStateRequest;

        public ColorimeterRequest()
        {
            //transferRequest = false;
            //testFileVersionRequest = false;
            //firmwareVersionRequest = false;
            //deviceStateRequest = false;
        }

    }

    [Flags]
    public enum ColorimeterRequestType
    {
        NotDefined = 0,
        Transfer = 1,
        FirmwareVersion = 2,
        TestFileVersion = 4,
        DeviceState = 8,
        GetUserTestsFile = 16,
    }
}
