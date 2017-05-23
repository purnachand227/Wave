﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace System.ComponentModel
{
    /// <summary>
    ///     Provides a STA (Single Thread Apartment) approach to running long tasks in the background using a
    ///     <see cref="BackgroundWorker" />.
    /// </summary>
    public static class Task
    {
        #region Fields

        /// <summary>
        ///     The current single apartment synchronization context.
        /// </summary>
        private static readonly SynchronizationContext Context = new STASynchronizationContext();

        /// <summary>
        ///     The threads
        /// </summary>
        private static long _Threads;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether there are active threads (those that have not been disposed).
        /// </summary>
        /// <value>
        ///     <c>true</c> if there are active threads; otherwise, <c>false</c>.
        /// </value>
        public static bool IsBusy
        {
            get { return Interlocked.Read(ref _Threads) > 0; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Runs the background process on a <see cref="BackgroundWorker" /> thread using the specified arguments that are
        ///     passed to the methods.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        public static void Run(Action task, Action complete)
        {
            Run(task, complete, null);
        }

        /// <summary>
        ///     Runs the background process on a <see cref="BackgroundWorker" /> thread using the specified arguments that are
        ///     passed to the methods.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <param name="error">The delegate that handles any unhandled exceptions that occured during execution.</param>
        public static void Run(Action task, Action complete, Action<Exception> error)
        {
            var thread = Run(task, Context);
            thread.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Error == null)
                    complete();
                else
                    error(e.Error);

                thread.Dispose();
            };
        }

        /// <summary>
        ///     Runs the background process on a <see cref="BackgroundWorker" /> thread using the specified arguments that are
        ///     passed to the methods.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <param name="error">The delegate that handles any unhandled exceptions that occured during execution.</param>
        public static void Run<TResult>(Func<TResult> task, Action<TResult> complete, Action<Exception> error)
        {
            var thread = Run(task, Context);
            thread.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Error == null)
                {
                    var result = (TResult)e.Result;
                    complete(result);
                }
                else
                    error(e.Error);

                thread.Dispose();
            };
        }

        /// <summary>
        ///     Runs the background process on a <see cref="BackgroundWorker" /> thread using the specified arguments that are
        ///     passed to the methods.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <returns>
        ///     Returns a <see cref="BackgroundWorker" /> representing the executing task.
        /// </returns>
        public static BackgroundWorker Run(Action task)
        {
            return Run(task, Context);
        }

        /// <summary>
        ///     Runs the background process on a <see cref="BackgroundWorker" /> thread using the specified arguments that are
        ///     passed to the methods.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <returns>
        ///     Returns a <see cref="BackgroundWorker" /> representing the executing task.
        /// </returns>
        public static BackgroundWorker Run(Action task, SynchronizationContext synchronizationContext)
        {
            Func<bool> execute = () =>
            {
                task();

                return true;
            };

            return Run(execute, synchronizationContext);
        }

        /// <summary>
        ///     Waits for the action that is issued as a background process on a <see cref="BackgroundWorker" /> thread to
        ///     complete.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <returns>
        ///     Returns a <see cref="TimeSpan" /> representing the time waited for the task to complete.
        /// </returns>
        public static TimeSpan Wait(Action task)
        {
            return Wait(task, () => { });
        }

        /// <summary>
        /// Waits for the action that is issued as a background process on a <see cref="BackgroundWorker" /> thread to
        /// complete.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="milisecondsTimeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <returns>
        /// Returns a <see cref="TimeSpan" /> representing the time waited for the task to complete.
        /// </returns>
        public static TimeSpan Wait(Action task, int milisecondsTimeout)
        {
            return Wait(task, () => { }, milisecondsTimeout);
        }
        /// <summary>
        ///     Waits for the action that is issued as a background process on a <see cref="BackgroundWorker" /> thread to
        ///     complete.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <returns>
        ///     Returns a <see cref="TimeSpan" /> representing the time waited for the task to complete.
        /// </returns>
        public static TimeSpan Wait(Action task, Action complete)
        {
            return Wait(task, complete, Timeout.Infinite, Context);
        }

        /// <summary>
        /// Waits for the action that is issued as a background process on a <see cref="BackgroundWorker" /> thread to
        /// complete.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <param name="milisecondsTimeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <returns>
        /// Returns a <see cref="TimeSpan" /> representing the time waited for the task to complete.
        /// </returns>
        public static TimeSpan Wait(Action task, Action complete, int milisecondsTimeout)
        {
            return Wait(task, complete, milisecondsTimeout, Context);
        }

        /// <summary>
        /// Waits for the action that is issued as a background process on a <see cref="BackgroundWorker" /> thread to
        /// complete.
        /// </summary>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <param name="milisecondsTimeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <returns>
        /// Returns a <see cref="TimeSpan" /> representing the time waited for the task to complete.
        /// </returns>
        public static TimeSpan Wait(Action task, Action complete, int milisecondsTimeout, SynchronizationContext synchronizationContext)
        {
            Func<bool> execute = () =>
            {
                task();

                return true;
            };

            Exception error = null;

            Stopwatch timer = Stopwatch.StartNew();

            using (var done = new AutoResetEvent(false))
            {
                Run(execute, e => error = e, done, synchronizationContext);

                done.WaitOne(milisecondsTimeout);               
            }

            if (error != null)
                throw error;

            var time = timer.Elapsed;
            
            complete();

            return time;
        }


        /// <summary>
        ///     Waits for all of the actions (that are issued as a background process on a <see cref="BackgroundWorker" /> thread)
        ///     to complete.
        /// </summary>
        /// <param name="tasks">The delegates that handles the execution on the work.</param>
        /// <returns>Returns a <see cref="TimeSpan" /> representing the time waited for the tasks to complete.</returns>
        public static TimeSpan WaitAll(IEnumerable<Action> tasks)
        {
            return WaitAll(tasks, () => { });
        }

        /// <summary>
        ///     Waits for all of the actions (that are issued as a background process on a <see cref="BackgroundWorker" /> thread)
        ///     to complete.
        /// </summary>
        /// <param name="tasks">The delegates that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <returns>
        ///     Returns a <see cref="TimeSpan" /> representing the time waited for the tasks to complete.
        /// </returns>
        public static TimeSpan WaitAll(IEnumerable<Action> tasks, Action complete)
        {
            return WaitAll(tasks, complete, Context);
        }

        /// <summary>
        ///     Waits for all of the actions (that are issued as a background process on a <see cref="BackgroundWorker" /> thread)
        ///     to complete.
        /// </summary>
        /// <param name="tasks">The delegates that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <returns>
        ///     Returns a <see cref="TimeSpan" /> representing the time waited for the tasks to complete.
        /// </returns>
        public static TimeSpan WaitAll(IEnumerable<BackgroundWorker> tasks, Action complete)
        {
            // Calling the WaitHandle method must be done from a multithreaded apartment (MTA) thread. 
            // To launch multiple synchronized tasks, you first have to launch a worker thread that, in turn, will run the multiple threads.
            return Wait(() =>
            {
                var done = new List<WaitHandle>();

                foreach (var t in tasks)
                {
                    AutoResetEvent wait = new AutoResetEvent(false);

                    if (t.IsBusy)
                    {
                        t.RunWorkerCompleted -= null;
                        t.RunWorkerCompleted += (sender, e) => { wait.Set(); };
                    }
                    else
                    {
                        wait.Set();
                    }

                    done.Add(wait);
                }

                WaitHandle.WaitAll(done.ToArray());
            },
                complete,
                Timeout.Infinite,
                new SynchronizationContext());
        }

        /// <summary>
        ///     Waits for all of the actions (that are issued as a background process on a <see cref="BackgroundWorker" /> thread)
        ///     to complete.
        /// </summary>
        /// <param name="tasks">The delegates that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <returns>
        ///     Returns a <see cref="TimeSpan" /> representing the time waited for the tasks to complete.
        /// </returns>
        public static TimeSpan WaitAll(IEnumerable<Action> tasks, Action complete, SynchronizationContext synchronizationContext)
        {
            List<Exception> errors = new List<Exception>();

            // Calling the WaitHandle method must be done from a multithreaded apartment (MTA) thread. 
            // To launch multiple synchronized tasks, you first have to launch a worker thread that, in turn, will run the multiple threads.
            var timer = Wait(() =>
            {
                var done = new List<WaitHandle>();

                foreach (var t in tasks)
                {
                    AutoResetEvent wait = new AutoResetEvent(false);
                    Action task = t;

                    Func<bool> execute = () =>
                    {
                        task();

                        return true;
                    };

                    done.Add(wait);

                    Run(execute, e => errors.Add(e), wait, synchronizationContext);
                }

                WaitHandle.WaitAll(done.ToArray());
            },
                complete,
                Timeout.Infinite,
                new SynchronizationContext());

            if (errors.Any())
            {
                throw errors.Aggregate(errors.First(), (current, e) => new Exception(current.Message, e));
            }

            return timer;
        }

        /// <summary>
        ///     Waits for any of the actions (that are issued as a background process on a <see cref="BackgroundWorker" /> thread)
        ///     to complete.
        /// </summary>
        /// <param name="tasks">The delegates that handles the execution on the work.</param>
        /// <param name="complete">The delegate that is called when the task completes.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <returns>
        ///     Returns a <see cref="TimeSpan" /> representing the time waited for the tasks to complete.
        /// </returns>
        public static TimeSpan WaitAny(IEnumerable<Action> tasks, Action complete, SynchronizationContext synchronizationContext)
        {
            List<Exception> errors = new List<Exception>();

            // Calling the WaitHandle method must be done from a multithreaded apartment (MTA) thread. 
            // To launch multiple synchronized tasks, you first have to launch a worker thread that, in turn, will run the multiple threads.
            var timer = Wait(() =>
            {
                var done = new List<WaitHandle>();

                foreach (var t in tasks)
                {
                    AutoResetEvent wait = new AutoResetEvent(false);
                    Action task = t;

                    Func<bool> execute = () =>
                    {
                        task();

                        return true;
                    };

                    Action<Exception> error = (e) => { errors.Add(e); };

                    done.Add(wait);

                    Run(execute, error, wait, synchronizationContext);
                }

                WaitHandle.WaitAll(done.ToArray());
            },
                complete,
                Timeout.Infinite,
                new SynchronizationContext());

            if (errors.Any())
            {
                throw errors.Aggregate(errors.First(), (current, e) => new Exception(current.Message, e));
            }

            return timer;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Runs the background process on a <see cref="BackgroundWorker" /> thread using the specified arguments that are
        ///     passed to the methods.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <returns>
        ///     Returns a <see cref="BackgroundWorker" /> representing the executing task.
        /// </returns>
        /// <exception cref="ArgumentNullException">task</exception>
        private static BackgroundWorker Run<TResult>(Func<TResult> task, SynchronizationContext synchronizationContext)
        {
            return Run(task, null, null, synchronizationContext);
        }

        /// <summary>
        ///     Runs the background process on a <see cref="BackgroundWorker" /> thread using the specified arguments that are
        ///     passed to the methods.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="task">The delegate that handles the execution on the work.</param>
        /// <param name="error">The delegate that handles any unhandled exceptions that occured during execution.</param>
        /// <param name="done">The handle that notifies a waiting thread that an event has occurred.</param>
        /// <param name="synchronizationContext">The synchronization context.</param>
        /// <returns>
        ///     Returns a <see cref="BackgroundWorker" /> representing the executing task.
        /// </returns>
        /// <exception cref="ArgumentNullException">task</exception>
        private static BackgroundWorker Run<TResult>(Func<TResult> task, Action<Exception> error, AutoResetEvent done, SynchronizationContext synchronizationContext)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            Interlocked.Increment(ref _Threads);

            AsyncOperationManager.SynchronizationContext = synchronizationContext ?? SynchronizationContext.Current;

            var thread = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };

            thread.DoWork += (sender, e) => { e.Result = task(); };

            if (done != null)
            {
                thread.RunWorkerCompleted += (sender, e) =>
                {
                    done.Set();

                    if (e.Error != null && error != null)
                        error(e.Error);

                    thread.Dispose();
                };
            }

            thread.Disposed += (sender, args) => { Interlocked.Decrement(ref _Threads); };

            thread.RunWorkerAsync();

            return thread;
        }

        #endregion
    }

    /// <summary>
    ///     A single threaded apartment synchornization context.
    /// </summary>
    /// <seealso cref="System.Threading.SynchronizationContext" />
    public class STASynchronizationContext : SynchronizationContext
    {
        #region Public Methods

        /// <summary>
        ///     When overridden in a derived class, dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(SendOrPostCallback d, object state)
        {
            Thread t = new Thread(d.Invoke);
            t.SetApartmentState(ApartmentState.STA);
            t.Start(state);
        }

        #endregion
    }
}