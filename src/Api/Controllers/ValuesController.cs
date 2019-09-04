using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionClient _sessionClient;
        private readonly IMessageSender _messageSender;

        public ValuesController(IConfiguration configuration , ISessionClient sessionClient, IMessageSender messageSender)
        {
            _configuration = configuration;
            _sessionClient = sessionClient;
            _messageSender = messageSender;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var log = new List<string>();

            var sessionId = Guid.NewGuid().ToString();
            log.Add($"Creating session {sessionId}");

            IMessageSession session = await _sessionClient.AcceptMessageSessionAsync(sessionId);
            Message response = null;
            var t = session.ReceiveAsync(new TimeSpan(0, 5, 0))
                           .ContinueWith(m => response = m.Result)
                           .ContinueWith(m => log.Add($"received response from service for {sessionId}"));

            var message = new Message(Encoding.UTF8.GetBytes("test message"))
                            {
                                SessionId = sessionId
                            };
            await _messageSender.SendAsync(message);
            log.Add($"Sent work request for {sessionId}");

            await t;
            await session.CompleteAsync(response.SystemProperties.LockToken);
            log.Add($"Completed session: {sessionId}");

            return log;
        }

    }
}
