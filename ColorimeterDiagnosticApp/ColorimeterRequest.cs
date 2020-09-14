using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterRequest
    {
        public ColorimeterRequestTypes requestInfo;
    }

    public enum ColorimeterRequestTypes
    {
        Transfer,
        FirmwareVersion,
        TestFileVersion
    }
}
