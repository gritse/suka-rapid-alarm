using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SukaRapidAlarm.Core.Abstractions;
using SukaRapidAlarm.Core.Domain;

namespace SukaRapidAlarm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmController : ControllerBase
    {
        private readonly IAlarmService _alarmService;
        private readonly ILogger<AlarmController> _logger;

        public AlarmController(IAlarmService alarmService, ILogger<AlarmController> logger)
        {
            _alarmService = alarmService;
            _logger = logger;
        }

        [HttpGet("poll/{soldierId}")]
        public PollAlarmMessage Poll([FromRoute] Guid soldierId, [FromQuery] int timeout = 20)
        {
            _logger.LogDebug($"Soldier is waiting for the order: Id: {soldierId}, Timeout: {timeout}");
            return _alarmService.GetOrder(soldierId, timeout);
        }

        [HttpPost("confirm/{soldierId}")]
        public void Confirm([FromRoute] Guid soldierId)
        {
            _logger.LogDebug($"Soldier has confirmed order: Id: {soldierId}");
            _alarmService.ConfirmOrder(soldierId);
        }

        [HttpPost("throw/{soldierId}")]
        public void ThrowMessage([FromRoute] Guid soldierId, [FromBody] ThrowAlarmMessage message)
        {
            _logger.LogDebug($"Soldier has sent message: Id: {soldierId}, Message Text: {message?.Message}");
            _alarmService.ThrowMessage(soldierId, message);
        }
    }
}