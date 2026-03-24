using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Services
{
    public class SqsService
    {
        private readonly AmazonSQSClient _sqsClient;
        private readonly string _filaUsuarioUrl;
        private readonly string _region;

        public SqsService(IConfiguration config)
        {
            _filaUsuarioUrl = config["AWS:SQS:UsuarioCriado"];
            _region = config["AWS:Region"];
            _sqsClient = new AmazonSQSClient(RegionEndpoint.GetBySystemName(_region));
        }
        public async Task EnviarUsuarioCriadoAsync(object mensagem)
        {
            await _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _filaUsuarioUrl,
                MessageBody = JsonSerializer.Serialize(mensagem)
            });
        }
    }
}
