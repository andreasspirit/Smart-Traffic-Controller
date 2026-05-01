using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTrafficController
{
    public interface ITimeManager
    {
        string GetStatus();
        bool Delay(int time);
    }
}
