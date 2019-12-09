using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Default.MService
{
    public interface IMService : IService
    {
        Task<Guid> CreateAsync(string type, CancellationToken cancellationToken);
    }
}
