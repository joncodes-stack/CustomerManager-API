using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CustomerManager.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CustomerManager.Infra.Messaging
{
    public class CustomerEventPublisher : ICustomerEventPublisher
    {
        private readonly IAmazonSimpleNotificationService _sns;
        private readonly string _topicArn;

        public CustomerEventPublisher(IAmazonSimpleNotificationService sns, IConfiguration config)
        {
            _sns = sns;
            _topicArn = config["AWS:SNS:ContaEventosTopicArn"];
        }

        public async Task PublicarAsync(CustomerEventMessage evento)
        {
            var mensagem = JsonSerializer.Serialize(evento);

            await _sns.PublishAsync(new PublishRequest
            {
                TopicArn = _topicArn,
                Message = mensagem,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["TipoEvento"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = evento.TipoEvento
                    }
                }
            });
        }
    }
}
