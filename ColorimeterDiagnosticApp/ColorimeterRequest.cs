using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterRequest
    {
        public ColorimeterRequestTypes colorimeterRequestType;


        public string taylorTestFile;

        //File testFile;
    }

    public enum ColorimeterRequestTypes
    {
        Transfer,
        FirmwareVersion,
        TestFileVersion
    }
}
