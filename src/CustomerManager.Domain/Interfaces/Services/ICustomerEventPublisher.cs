using System.Threading.Tasks;

namespace CustomerManager.Domain.Interfaces.Services
{
    public interface ICustomerEventPublisher
    {
        Task PublicarAsync(CustomerEventMessage evento);
    }

    public class CustomerEventMessage
    {
        public string TipoEvento { get; set; }
        public string CustomerId { get; set; }
        public DateTime DataEvento { get; set; }
        public string? Detalhes { get; set; }
    }
}
