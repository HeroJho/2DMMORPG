using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class JobSerializer
    {
        JobTimer _timer = new JobTimer();

        Queue<IJob> _jobQueue = new Queue<IJob>();
        object _lock = new object();

        public IJob PushAfter(int tickAfter, Action action) { return PushAfter(tickAfter, new Job(action)); }
        public IJob PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { return PushAfter(tickAfter, new Job<T1>(action, t1)); }
        public IJob PushAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { return PushAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
        public IJob PushAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { return PushAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }

        public IJob PushAfter(int tickAfter, IJob job)
        {
            _timer.Push(job, tickAfter);
            return job;
        }

        public void Push(Action action) { Push(new Job(action)); }
        public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
        public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
        public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }


        public void Push(IJob job)
        {
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
            }
        }

        public void Flush()
        {
            _timer.Flush();

            while (true)
            {
                IJob action = Pop();
                if (action == null)
                    return;

                action.Execute();
            }
        }

        IJob Pop()
        {
            lock (_lock)
            {
                if(_jobQueue.Count == 0)
                    return null;

                return _jobQueue.Dequeue();
            }
        }
    }
}
