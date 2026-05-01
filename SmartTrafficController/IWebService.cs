using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTrafficController
{
    public interface IWebService
    {

        bool FaultDetected(bool on);
        void LogEngineerRequired(string  type); 
    }
}
