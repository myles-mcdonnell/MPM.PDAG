using System;
using System.Diagnostics;
using System.Threading;

namespace MPM.PDAG.UnitTests
{
    public class EventRegister
    {
        private readonly object _lock = new object();
        public int Count { get; private set; }

        public void Increment()
        {
            lock (_lock)
            {
                Count++;
                Monitor.Pulse(_lock);
            }
        }

        public static EventRegister operator ++(EventRegister counter)
        {
            counter.Count++;
            return counter;
        }

        public static implicit operator int(EventRegister counter)
        {
            return counter.Count;
        }

        public bool HasFired { get { return Count > 0; } }

        public void Wait(int unitCount)
        {
            Wait(unitCount, TimeSpan.MinValue);
        }

        public override string ToString()
        {
            return string.Format("Event Count : {0}", Count);
        }

        public void Wait(int unitCount, TimeSpan timeout)
        {
            lock (_lock)
            {
                var start = DateTime.Now;

                while (Count < unitCount)
                {
                    if (timeout != TimeSpan.MinValue && !Debugger.IsAttached)
                    {
                        Monitor.Wait(_lock, timeout);
                        
                        if (!Debugger.IsAttached &DateTime.Now-start>timeout)
                            throw new TimeoutException();
                    }
                    else
                        Monitor.Wait(_lock);
                }
            }
        }

        public static void WaitInSequence(TimeSpan timeout, params EventRegister[] eventRegisters)
        {
            foreach (var eventRegister in eventRegisters)
                eventRegister.Wait(1, timeout);
        }
    }
}
