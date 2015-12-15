// ReSharper disable NotAccessedField.Local
#pragma warning disable 169
namespace NServiceBus.AzureServiceBus.AcceptanceTests
{
    using System;
    using System.Runtime.InteropServices;
    using GuerrillaNtp;

    public class TimeSynchronization
    {
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        public void Sync()
        {
            // Call the native GetSystemTime method 
            // with the defined structure.
            var systime = new SYSTEMTIME();

            TimeSpan offset;
            using (var ntp = new NtpClient("pool.ntp.org"))
            {
                ntp.Timeout = TimeSpan.FromSeconds(5);
                var ntpResponse = ntp.Query();
                offset = ntpResponse.CorrectionOffset;
            }
            var accurateTime = DateTime.UtcNow + offset;

            systime.wYear = (ushort)accurateTime.Year;
            systime.wMonth = (ushort)accurateTime.Month;
            systime.wDay = (ushort)accurateTime.Day;
            systime.wHour = (ushort)accurateTime.Hour;
            systime.wMinute = (ushort)accurateTime.Minute;
            systime.wSecond = (ushort)accurateTime.Second;
            systime.wMilliseconds = (ushort)accurateTime.Millisecond;

            SetSystemTime(ref systime);
        }
    }
}
