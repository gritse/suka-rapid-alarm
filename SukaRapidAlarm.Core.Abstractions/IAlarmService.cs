using System;
using System.Threading.Tasks;
using SukaRapidAlarm.Core.Domain;

namespace SukaRapidAlarm.Core.Abstractions
{
    public interface IAlarmService
    {
        PollAlarmMessage GetOrder(Guid soldierId, int timeout);
        void ThrowMessage(Guid soldierId, ThrowAlarmMessage message);
        void ConfirmOrder(Guid soldierId);
    }
}