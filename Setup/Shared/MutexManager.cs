using System;
using System.Security.AccessControl;
using System.Threading;

namespace Eulg.Setup.Shared
{
    public class MutexManager
    {
        private static MutexManager _instance;

        public static MutexManager Instance => _instance ?? (_instance = new MutexManager());

        private MutexManager()
        {
        }

        private string MutexSystemName(string name)
        {
            return $"SETUP[{name}]";
        }

        private Mutex GetMutex(string name)
        {
            var sysname = MutexSystemName(name);

            try
            {
                return Mutex.OpenExisting(sysname, MutexRights.FullControl);
            }
            catch
            {
                return new Mutex(false, sysname);
            }
        }

        /// <summary>
        /// Check if the named resource is currently available. It is not safe to try to acquire the mutex after calling this method (race condition),
        /// use the Acquire method with a timeout to try to acquire without blocking.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="AbandonedMutexException"></exception>
        public bool IsAvailable(string name)
        {
            var mutex = GetMutex(name);

            var result = mutex.WaitOne(0);
            if (result)
            {
                mutex.ReleaseMutex(); // Nobody was owning it, so this call acquired it
            }

            return result;
        }

        /// <summary>
        /// Attempt to acquire exclusive ownership over the identified resource within the given amount of time. If acquisition failed, null is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="AbandonedMutexException"></exception>
        public Mutex Acquire(string name, TimeSpan? timeout = null)
        {
            var mutex = GetMutex(name);

            var result = timeout.HasValue ? mutex.WaitOne(timeout.Value) : mutex.WaitOne();
            if (result)
            {
                return mutex; // Only return the mutex if it was successfully acquired in the given time span
            }

            return null;
        }

        /// <summary>
        /// Attempt to acquire exclusive ownership over the identified resource within the given amount of time. If acquisition failed, null is returned.
        /// Catch and hide AbandonedMutexException.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Mutex AcquireIgnoreAbandoned(string name, TimeSpan? timeout = null)
        {
            var mutex = GetMutex(name);

            try
            {
                var result = timeout.HasValue ? mutex.WaitOne(timeout.Value) : mutex.WaitOne();
                if (result)
                {
                    return mutex; // Only return the mutex if it was successfully acquired in the given time span
                }
            }
            catch (AbandonedMutexException)
            {
                return mutex;
            }

            return null;
        }
    }
}
