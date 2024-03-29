/***************************************************************************
*                                 Timer.cs
*                            -------------------
*   begin                : May 1, 2002
*   copyright            : (C) The RunUO Software Team
*   email                : info@runuo.com
*
*   $Id: Timer.cs 816 2012-01-04 08:45:24Z asayre $
*
***************************************************************************/

/***************************************************************************
*
*   This program is free software; you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation; either version 2 of the License, or
*   (at your option) any later version.
*
***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Server.Diagnostics;

namespace Server
{
    public enum TimerPriority
    {
        EveryTick,
        TenMS,
        TwentyFiveMS,
        FiftyMS,
        TwoFiftyMS,
        OneSecond,
        FiveSeconds,
        OneMinute
    }

    public delegate void TimerCallback();
    public delegate void TimerStateCallback(object state);
    public delegate void TimerStateCallback<T>(T state);

    public class Timer
    {
        private DateTime m_Next;
        private TimeSpan m_Delay;
        private TimeSpan m_Interval;
        private bool m_Running;
        private readonly int m_Count;

        private int m_Index;
        private TimerPriority m_Priority;
        private List<Timer> m_List;
        private bool m_PrioritySet;

        private static string FormatDelegate(Delegate callback)
        {
            if (callback == null)
                return "null";

            return String.Format("{0}.{1}", callback.Method.DeclaringType.FullName, callback.Method.Name);
        }

        public static void DumpInfo(TextWriter tw)
        {
            TimerThread.DumpInfo(tw);
        }

        public TimerPriority Priority
        {
            get
            {
                return this.m_Priority;
            }
            set
            {
                if (!this.m_PrioritySet)
                    this.m_PrioritySet = true;

                if (this.m_Priority != value)
                {
                    this.m_Priority = value;

                    if (this.m_Running)
                        TimerThread.PriorityChange(this, (int)this.m_Priority);
                }
            }
        }

        public DateTime Next
        {
            get
            {
                return this.m_Next;
            }
        }

        public TimeSpan Delay
        {
            get
            {
                return this.m_Delay;
            }
            set
            {
                this.m_Delay = value;
            }
        }

        public TimeSpan Interval
        {
            get
            {
                return this.m_Interval;
            }
            set
            {
                this.m_Interval = value;
            }
        }

        public bool Running
        {
            get
            {
                return this.m_Running;
            }
            set
            {
                if (value)
                {
                    this.Start();
                }
                else
                {
                    this.Stop();
                }
            }
        }

        public TimerProfile GetProfile()
        {
            if (!Core.Profiling)
            {
                return null;
            }

            string name = this.ToString();

            if (name == null)
            {
                name = "null";
            }

            return TimerProfile.Acquire(name);
        }

        public class TimerThread
        {
            private static readonly Queue m_ChangeQueue = Queue.Synchronized(new Queue());

            private static readonly DateTime[] m_NextPriorities = new DateTime[8];
            private static readonly TimeSpan[] m_PriorityDelays = new TimeSpan[8]
            {
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(10.0),
                TimeSpan.FromMilliseconds(25.0),
                TimeSpan.FromMilliseconds(50.0),
                TimeSpan.FromMilliseconds(250.0),
                TimeSpan.FromSeconds(1.0),
                TimeSpan.FromSeconds(5.0),
                TimeSpan.FromMinutes(1.0)
            };

            private static readonly List<Timer>[] m_Timers = new List<Timer>[8]
            {
                new List<Timer>(),
                new List<Timer>(),
                new List<Timer>(),
                new List<Timer>(),
                new List<Timer>(),
                new List<Timer>(),
                new List<Timer>(),
                new List<Timer>(),
            };

            public static void DumpInfo(TextWriter tw)
            {
                for (int i = 0; i < 8; ++i)
                {
                    tw.WriteLine("Priority: {0}", (TimerPriority)i);
                    tw.WriteLine();

                    Dictionary<string, List<Timer>> hash = new Dictionary<string, List<Timer>>();

                    for (int j = 0; j < m_Timers[i].Count; ++j)
                    {
                        Timer t = m_Timers[i][j];

                        string key = t.ToString();

                        List<Timer> list;
                        hash.TryGetValue(key, out list);

                        if (list == null)
                            hash[key] = list = new List<Timer>();

                        list.Add(t);
                    }

                    foreach (KeyValuePair<string, List<Timer>> kv in hash)
                    {
                        string key = kv.Key;
                        List<Timer> list = kv.Value;

                        tw.WriteLine("Type: {0}; Count: {1}; Percent: {2}%", key, list.Count, (int)(100 * (list.Count / (double)m_Timers[i].Count)));
                    }

                    tw.WriteLine();
                    tw.WriteLine();
                }
            }

            private class TimerChangeEntry
            {
                public Timer m_Timer;
                public int m_NewIndex;
                public bool m_IsAdd;

                private TimerChangeEntry(Timer t, int newIndex, bool isAdd)
                {
                    this.m_Timer = t;
                    this.m_NewIndex = newIndex;
                    this.m_IsAdd = isAdd;
                }

                public void Free()
                {
                    //m_InstancePool.Enqueue( this );
                }

                private static readonly Queue<TimerChangeEntry> m_InstancePool = new Queue<TimerChangeEntry>();

                public static TimerChangeEntry GetInstance(Timer t, int newIndex, bool isAdd)
                {
                    TimerChangeEntry e;

                    if (m_InstancePool.Count > 0)
                    {
                        e = m_InstancePool.Dequeue();

                        if (e == null)
                            e = new TimerChangeEntry(t, newIndex, isAdd);
                        else
                        {
                            e.m_Timer = t;
                            e.m_NewIndex = newIndex;
                            e.m_IsAdd = isAdd;
                        }
                    }
                    else
                    {
                        e = new TimerChangeEntry(t, newIndex, isAdd);
                    }

                    return e;
                }
            }

            public TimerThread()
            {
            }

            public static void Change(Timer t, int newIndex, bool isAdd)
            {
                m_ChangeQueue.Enqueue(TimerChangeEntry.GetInstance(t, newIndex, isAdd));
                m_Signal.Set();
            }

            public static void AddTimer(Timer t)
            {
                Change(t, (int)t.Priority, true);
            }

            public static void PriorityChange(Timer t, int newPrio)
            {
                Change(t, newPrio, false);
            }

            public static void RemoveTimer(Timer t)
            {
                Change(t, -1, false);
            }

            private static void ProcessChangeQueue()
            {
                while (m_ChangeQueue.Count > 0)
                {
                    TimerChangeEntry tce = (TimerChangeEntry)m_ChangeQueue.Dequeue();
                    Timer timer = tce.m_Timer;
                    int newIndex = tce.m_NewIndex;

                    if (timer.m_List != null)
                        timer.m_List.Remove(timer);

                    if (tce.m_IsAdd)
                    {
                        timer.m_Next = DateTime.Now + timer.m_Delay;
                        timer.m_Index = 0;
                    }

                    if (newIndex >= 0)
                    {
                        timer.m_List = m_Timers[newIndex];
                        timer.m_List.Add(timer);
                    }
                    else
                    {
                        timer.m_List = null;
                    }

                    tce.Free();
                }
            }

            private static readonly AutoResetEvent m_Signal = new AutoResetEvent(false);
            public static void Set()
            {
                m_Signal.Set();
            }

            public void TimerMain()
            {
                DateTime now;
                int i, j;
                bool loaded;

                while (!Core.Closing)
                {
                    ProcessChangeQueue();

                    loaded = false;

                    for (i = 0; i < m_Timers.Length; i++)
                    {
                        now = DateTime.Now;
                        if (now < m_NextPriorities[i])
                            break;

                        m_NextPriorities[i] = now + m_PriorityDelays[i];

                        for (j = 0; j < m_Timers[i].Count; j++)
                        {
                            Timer t = m_Timers[i][j];

                            if (!t.m_Queued && now > t.m_Next)
                            {
                                t.m_Queued = true;

                                lock (m_Queue)
                                    m_Queue.Enqueue(t);

                                loaded = true;
									
                                if (t.m_Count != 0 && (++t.m_Index >= t.m_Count))
                                {
                                    t.Stop();
                                }
                                else
                                {
                                    t.m_Next = now + t.m_Interval;
                                }
                            }
                        }
                    }

                    if (loaded)
                        Core.Set();

                    m_Signal.WaitOne(10, false);
                }
            }
        }

        private static readonly Queue<Timer> m_Queue = new Queue<Timer>();
        private static int m_BreakCount = 20000;

        public static int BreakCount
        {
            get
            {
                return m_BreakCount;
            }
            set
            {
                m_BreakCount = value;
            }
        }

        private static int m_QueueCountAtSlice;

        private bool m_Queued;

        public static void Slice()
        {
            lock (m_Queue)
            {
                m_QueueCountAtSlice = m_Queue.Count;

                int index = 0;

                while (index < m_BreakCount && m_Queue.Count != 0)
                {
                    Timer t = m_Queue.Dequeue();
                    TimerProfile prof = t.GetProfile();

                    if (prof != null)
                    {
                        prof.Start();
                    }

                    t.OnTick();
                    t.m_Queued = false;
                    ++index;

                    if (prof != null)
                    {
                        prof.Finish();
                    }
                }
            }
        }

        public Timer(TimeSpan delay) : this(delay, TimeSpan.Zero, 1)
        {
        }

        public Timer(TimeSpan delay, TimeSpan interval) : this(delay, interval, 0)
        {
        }

        public virtual bool DefRegCreation
        {
            get
            {
                return true;
            }
        }

        public virtual void RegCreation()
        {
            TimerProfile prof = this.GetProfile();

            if (prof != null)
            {
                prof.Created++;
            }
        }

        public Timer(TimeSpan delay, TimeSpan interval, int count)
        {
            this.m_Delay = delay;
            this.m_Interval = interval;
            this.m_Count = count;

            if (!this.m_PrioritySet)
            {
                if (count == 1)
                {
                    this.m_Priority = ComputePriority(delay);
                }
                else
                {
                    this.m_Priority = ComputePriority(interval);
                }
                this.m_PrioritySet = true;
            }

            if (this.DefRegCreation)
                this.RegCreation();
        }

        public override string ToString()
        {
            return this.GetType().FullName;
        }

        public static TimerPriority ComputePriority(TimeSpan ts)
        {
            if (ts >= TimeSpan.FromMinutes(1.0))
                return TimerPriority.FiveSeconds;

            if (ts >= TimeSpan.FromSeconds(10.0))
                return TimerPriority.OneSecond;

            if (ts >= TimeSpan.FromSeconds(5.0))
                return TimerPriority.TwoFiftyMS;

            if (ts >= TimeSpan.FromSeconds(2.5))
                return TimerPriority.FiftyMS;

            if (ts >= TimeSpan.FromSeconds(1.0))
                return TimerPriority.TwentyFiveMS;

            if (ts >= TimeSpan.FromSeconds(0.5))
                return TimerPriority.TenMS;

            return TimerPriority.EveryTick;
        }

        #region DelayCall(..)

        public static Timer DelayCall(TimerCallback callback)
        {
            return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback);
        }

        public static Timer DelayCall(TimeSpan delay, TimerCallback callback)
        {
            return DelayCall(delay, TimeSpan.Zero, 1, callback);
        }

        public static Timer DelayCall(TimeSpan delay, TimeSpan interval, TimerCallback callback)
        {
            return DelayCall(delay, interval, 0, callback);
        }

        public static Timer DelayCall(TimeSpan delay, TimeSpan interval, int count, TimerCallback callback)
        {
            Timer t = new DelayCallTimer(delay, interval, count, callback);

            if (count == 1)
                t.Priority = ComputePriority(delay);
            else
                t.Priority = ComputePriority(interval);

            t.Start();

            return t;
        }

        public static Timer DelayCall(TimerStateCallback callback, object state)
        {
            return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state);
        }

        public static Timer DelayCall(TimeSpan delay, TimerStateCallback callback, object state)
        {
            return DelayCall(delay, TimeSpan.Zero, 1, callback, state);
        }

        public static Timer DelayCall(TimeSpan delay, TimeSpan interval, TimerStateCallback callback, object state)
        {
            return DelayCall(delay, interval, 0, callback, state);
        }

        public static Timer DelayCall(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state)
        {
            Timer t = new DelayStateCallTimer(delay, interval, count, callback, state);

            if (count == 1)
                t.Priority = ComputePriority(delay);
            else
                t.Priority = ComputePriority(interval);

            t.Start();

            return t;
        }

        #endregion

        #region DelayCall<T>(..)
        public static Timer DelayCall<T>(TimerStateCallback<T> callback, T state)
        {
            return DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback, state);
        }

        public static Timer DelayCall<T>(TimeSpan delay, TimerStateCallback<T> callback, T state)
        {
            return DelayCall(delay, TimeSpan.Zero, 1, callback, state);
        }

        public static Timer DelayCall<T>(TimeSpan delay, TimeSpan interval, TimerStateCallback<T> callback, T state)
        {
            return DelayCall(delay, interval, 0, callback, state);
        }

        public static Timer DelayCall<T>(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state)
        {
            Timer t = new DelayStateCallTimer<T>(delay, interval, count, callback, state);

            if (count == 1)
                t.Priority = ComputePriority(delay);
            else
                t.Priority = ComputePriority(interval);

            t.Start();

            return t;
        }

        #endregion

        #region DelayCall Timers
        private class DelayCallTimer : Timer
        {
            private readonly TimerCallback m_Callback;

            public TimerCallback Callback
            {
                get
                {
                    return this.m_Callback;
                }
            }

            public override bool DefRegCreation
            {
                get
                {
                    return false;
                }
            }

            public DelayCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerCallback callback) : base(delay, interval, count)
            {
                this.m_Callback = callback;
                this.RegCreation();
            }

            protected override void OnTick()
            {
                if (this.m_Callback != null)
                    this.m_Callback();
            }

            public override string ToString()
            {
                return String.Format("DelayCallTimer[{0}]", FormatDelegate(this.m_Callback));
            }
        }

        private class DelayStateCallTimer : Timer
        {
            private readonly TimerStateCallback m_Callback;
            private readonly object m_State;

            public TimerStateCallback Callback
            {
                get
                {
                    return this.m_Callback;
                }
            }

            public override bool DefRegCreation
            {
                get
                {
                    return false;
                }
            }

            public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback callback, object state) : base(delay, interval, count)
            {
                this.m_Callback = callback;
                this.m_State = state;

                this.RegCreation();
            }

            protected override void OnTick()
            {
                if (this.m_Callback != null)
                    this.m_Callback(this.m_State);
            }

            public override string ToString()
            {
                return String.Format("DelayStateCall[{0}]", FormatDelegate(this.m_Callback));
            }
        }

        private class DelayStateCallTimer<T> : Timer
        {
            private readonly TimerStateCallback<T> m_Callback;
            private readonly T m_State;

            public TimerStateCallback<T> Callback
            {
                get
                {
                    return this.m_Callback;
                }
            }

            public override bool DefRegCreation
            {
                get
                {
                    return false;
                }
            }

            public DelayStateCallTimer(TimeSpan delay, TimeSpan interval, int count, TimerStateCallback<T> callback, T state) : base(delay, interval, count)
            {
                this.m_Callback = callback;
                this.m_State = state;

                this.RegCreation();
            }

            protected override void OnTick()
            {
                if (this.m_Callback != null)
                    this.m_Callback(this.m_State);
            }

            public override string ToString()
            {
                return String.Format("DelayStateCall[{0}]", FormatDelegate(this.m_Callback));
            }
        }
        #endregion

        public void Start()
        {
            if (!this.m_Running)
            {
                this.m_Running = true;
                TimerThread.AddTimer(this);

                TimerProfile prof = this.GetProfile();

                if (prof != null)
                {
                    prof.Started++;
                }
            }
        }

        public void Stop()
        {
            if (this.m_Running)
            {
                this.m_Running = false;
                TimerThread.RemoveTimer(this);

                TimerProfile prof = this.GetProfile();

                if (prof != null)
                {
                    prof.Stopped++;
                }
            }
        }

        protected virtual void OnTick()
        {
        }
    }
}