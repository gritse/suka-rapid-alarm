using System;
using System.Diagnostics;
using NUnit.Framework;

namespace SukaRapidAlarm.Tests
{
    internal static class Helper
    {
        public static T EnsureTimeout<T>(Func<T> func, int timeoutSec)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = func();

            Assert.AreEqual(timeoutSec * 1000, stopwatch.ElapsedMilliseconds, 1000.0);
            return result;
        }
    }
}