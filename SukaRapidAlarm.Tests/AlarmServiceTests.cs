using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SukaRapidAlarm.Core;
using SukaRapidAlarm.Core.Domain;

namespace SukaRapidAlarm.Tests
{
    public class AlarmServiceTests
    {
        private AlarmService _alarmService;

        private Guid peetooh = Guid.NewGuid();
        private Guid hound = Guid.NewGuid();

        private PollAlarmMessage _testPollMessage = new PollAlarmMessage(true, "Test");
        private ThrowAlarmMessage _testThrowAlarmMessage = new ThrowAlarmMessage("Test");

        [SetUp]
        public void Setup()
        {
            _alarmService = new AlarmService();
        }

        [Test]
        public void NoMessage_Wait()
        {
            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 10), 10);
            Assert.AreEqual(PollAlarmMessage.Empty, order);
        }

        [Test]
        public void HasMessage_NoWait()
        {
            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 25), 0);
            Assert.AreEqual(_testPollMessage, order);
        }

        [Test]
        public void NoMessage_Wait_Then_ThrowMessage_InstantReturn()
        {
            var task = new TaskFactory().StartNew(() =>
            {
                return _alarmService.GetOrder(peetooh, 25);
            });

            Thread.Sleep(5000);

            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            var order = Helper.EnsureTimeout(() =>  task.GetAwaiter().GetResult(), 0);
            Assert.AreEqual(_testPollMessage, order);
        }

        [Test]
        public void NoMessage_Wait_Then_Return_UntilConfirmed()
        {
            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);

            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 25), 0);
            Assert.AreEqual(_testPollMessage, order);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 25), 0);
            Assert.AreEqual(_testPollMessage, order);
        }

        [Test]
        public void HasMessage_NoWait_Confirm_Wait()
        {
            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 25), 0);
            Assert.AreEqual(_testPollMessage, order);

            _alarmService.ConfirmOrder(peetooh);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);
        }

        [Test]
        public void HasMessage_NoWait_Confirm_Wait_ThrowMessage_NoWait()
        {
            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 25), 0);
            Assert.AreEqual(_testPollMessage, order);

            _alarmService.ConfirmOrder(peetooh);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);

            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 0);
            Assert.AreEqual(_testPollMessage, order);
        }

        [Test]
        public void NoMessage_Confirm_Wait()
        {
            _alarmService.ConfirmOrder(peetooh);

            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);
        }

        [Test]
        public void NoMessage_Confirm_Wait_Throw_NoWait()
        {
            _alarmService.ConfirmOrder(peetooh);

            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);

            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 0);
            Assert.AreEqual(_testPollMessage, order);
        }

        [Test]
        public void NoMessage_Wait_Throw_NoWait_ExpireMessage_Wait()
        {
            _alarmService.MessageExpireTimeout = TimeSpan.FromSeconds(5);

            var order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);

            _alarmService.ThrowMessage(hound, _testThrowAlarmMessage);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 0);
            Assert.AreEqual(_testPollMessage, order);

            Thread.Sleep(_alarmService.MessageExpireTimeout + TimeSpan.FromSeconds(1));

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(peetooh, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);

            order = Helper.EnsureTimeout(() => _alarmService.GetOrder(hound, 5), 5);
            Assert.AreEqual(PollAlarmMessage.Empty, order);
        }

    }
}