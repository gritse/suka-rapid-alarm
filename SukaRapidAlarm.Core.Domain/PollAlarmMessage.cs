namespace SukaRapidAlarm.Core.Domain
{
    public record PollAlarmMessage(bool HasMessage, string Message)
    {
        public static PollAlarmMessage Empty = new PollAlarmMessage(false, null);
    }
}