using CustomerManager.Infra.Messaging.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Application.Interfaces
{
    public interface ICustomerEventPublisher
    {
        Task PublicarAsync(CustomerEvent evento);
    }
}
