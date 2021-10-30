using System;
using System.Collections.Concurrent;
using System.Threading;
using SukaRapidAlarm.Core.Abstractions;
using SukaRapidAlarm.Core.Domain;

namespace SukaRapidAlarm.Core
{
    public class AlarmService : IAlarmService
    {
        private readonly ConcurrentDictionary<Guid, ManualResetEventSlim> _soldierConfirmationMap =
            new ConcurrentDictionary<Guid, ManualResetEventSlim>();

        private readonly object _sync = new object();
        private readonly Timer _expireMessageJob;

        private volatile ThrowAlarmMessage _alarmMessage;

        public AlarmService()
        {
            _expireMessageJob = new Timer(_ => ExpireMessageJob(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public TimeSpan MessageExpireTimeout { get; set; } = TimeSpan.FromMinutes(10);

        public PollAlarmMessage GetOrder(Guid soldierId, int timeout)
        {
            var alarmMessage = _alarmMessage;
            var hasMessage = alarmMessage is not null;

            var signal = _soldierConfirmationMap.GetOrAdd(soldierId, _ => new ManualResetEventSlim(hasMessage));
            var broadcastSignalReceived = signal.Wait(TimeSpan.FromSeconds(timeout));

            return broadcastSignalReceived
                ? new PollAlarmMessage(true, _alarmMessage.Message)
                : PollAlarmMessage.Empty;
        }

        public void ThrowMessage(Guid soldierId, ThrowAlarmMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            lock (_sync)
            {
                _alarmMessage = message;

                foreach (var manualResetEventSlim in _soldierConfirmationMap.Values)
                {
                    manualResetEventSlim.Set();
                }

                _expireMessageJob.Change(MessageExpireTimeout, Timeout.InfiniteTimeSpan);
            }
        }

        public void ConfirmOrder(Guid soldierId)
        {
            lock (_sync)
            {
                var signal = _soldierConfirmationMap.GetOrAdd(soldierId, _ => new ManualResetEventSlim());
                signal.Reset();
            }
        }

        private void ExpireMessageJob()
        {
            lock (_sync)
            {
                foreach (var manualResetEventSlim in _soldierConfirmationMap.Values)
                {
                    manualResetEventSlim.Reset();
                }

                _alarmMessage = null;
            }
        }
    }
}