using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ColorimeterDiagnosticApp
{
    public class ColorimeterRequest
    {

        public ColorimeterActionType ColorimeterRequestActionType;
    
    }

    [Flags]
    public enum ColorimeterActionType
    {
        NotDefined = 0,
        Transfer = 1,
        FirmwareVersion = 2,
        TestFileVersion = 4,
        DeviceState = 8,
        GetUserTestsFile = 16,
        GetTestResults = 32,
        UpdateUserTestsFile = 64,
        UpdateTaylorTestsFile = 128,
        UpdateFirmware = 256
    }
}
