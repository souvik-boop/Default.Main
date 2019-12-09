using Default.MService;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            IMService helloWorldClient = ServiceProxy.Create<IMService>(new Uri("fabric:/MyApplication/Default.MService"));
        }
    }
}
