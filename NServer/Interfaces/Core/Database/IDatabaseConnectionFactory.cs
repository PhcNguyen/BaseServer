using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServer.Interfaces.Core.Database
{
    internal interface IDatabaseConnectionFactory
    {
        IDatabaseConnection CreateConnection();
    }
}
