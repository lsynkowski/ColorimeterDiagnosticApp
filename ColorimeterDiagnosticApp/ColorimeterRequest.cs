﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterRequest
    {

        public ColorimeterRequestType ColorimeterRequestType;
    

        public ColorimeterRequest()
        {

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
        GetTestResults = 32,
    }
}
