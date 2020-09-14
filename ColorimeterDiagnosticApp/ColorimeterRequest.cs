using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterRequest
    {
        public ColorimeterRequestTypes requestInfo;
        public Boolean transferRequest;
        public Boolean testFileVersionRequest;
        public Boolean firmwareVersionRequest;
        public Boolean deviceStateRequest;

        public ColorimeterRequest()
        {
            transferRequest = false;
            testFileVersionRequest = false;
            firmwareVersionRequest = false;
            deviceStateRequest = false;
        }
    }

    public enum ColorimeterRequestTypes
    {
        Transfer,
        FirmwareVersion,
        TestFileVersion
    }
}
