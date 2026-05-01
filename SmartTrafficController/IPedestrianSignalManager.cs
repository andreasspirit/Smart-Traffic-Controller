using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTrafficController
{
    public interface IPedestrianSignalManager
    {
        string GetStatus();
        bool SetWait(bool on);
        bool SetWalk(bool on);
        bool SetAudible(bool on);
    }
}


