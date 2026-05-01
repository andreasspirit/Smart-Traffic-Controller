using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTrafficController
{
    public interface IVehicleSignalManager
    {
        string GetStatus();
        bool SetAllRed(bool on);
        bool SetAllGreen(bool on);

        bool LogEngineerRequired(bool needsEngineer);
    }
}
