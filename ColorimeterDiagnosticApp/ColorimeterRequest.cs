using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterRequest
    {

        public ColorimeterRequestType colorimeterRequestType;
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

    public enum ColorimeterRequestType
    {
        NotDefined,
        Transfer,
        FirmwareVersion,
        TestFileVersion,
        GetUserTestsFile
    }
}
