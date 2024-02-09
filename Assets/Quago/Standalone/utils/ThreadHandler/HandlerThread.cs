/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System.Threading;
using System.Collections.Generic;
using System;

public class HandlerThread
{
    protected object _threadLock = new object();
    protected object _dataLock = new object();
    protected List<Message> _messages;
    protected List<Timer> _timers;
    protected Thread _thread;
    protected Handler _handler;
    protected bool _pause, abort = false;
    public string Name { get; protected set; }

    public delegate void HandlerException(string threadName, Exception exception);

    public abstract class Handler
    {
        protected internal HandlerThread handlerThread;

        public abstract void HandleMessage(Message msg);

        public void SendMessage(Message message)
        {
            handlerThread.SendMessage(message);
        }

        public void SendMessageAtFrontOfQueue(Message message)
        {
            handlerThread.SendMessageAtFrontOfQueue(message);
        }

        public void SendEmptyMessage(int What)
        {
            handlerThread.SendEmptyMessage(What);
        }

        public void SendEmptyMessageDelayed(int What, long DelayMilliseconds)
        {
            handlerThread.SendEmptyMessageDelayed(What, DelayMilliseconds);
        }

        public void SendMessageDelayed(Message message, long DelayMilliseconds)
        {
            handlerThread.SendMessageDelayed(message, DelayMilliseconds);
        }

        public void RemoveMessages(int What)
        {
            handlerThread.SendEmptyMessage(What);
        }

        public void RemoveAllMessages()
        {
            handlerThread.RemoveAllMessages();
        }

        public void QuitSafely()
        {
            handlerThread.QuitSafely();
        }
    }

    protected HandlerException ExceptionDelegate { private get; set; }

    public HandlerThread(string name, Handler handler, HandlerException ExceptionDelegate) : this(name, handler)
    {
        this.ExceptionDelegate = ExceptionDelegate;
    }

    public HandlerThread(string name, Handler handler)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        Name = name;
        _handler = handler;
        _handler.handlerThread = this;
        _pause = true;
        _messages = new List<Message>();
        _timers = new List<Timer>();
        _thread = new Thread(RunThread)
        {
            Name = name + "-WorkerThread"
        };
        _thread.Start();
    }

    /**
     * Runs on a background Thread.
     */
    private void RunThread()
    {
        try
        {
            Message msg;
            while (!abort)
            {
                if (_pause)
                    lock (_threadLock)
                    {
                        Monitor.Wait(_threadLock);
                    }

                // Pull first Msg
                lock (_dataLock)
                {
                    if (_messages.Count == 0)
                    {
                        //Nothing to do so pause work.
                        _pause = true;
                        continue;
                    }
                    msg = _messages[0];
                    _messages.RemoveAt(0);
                }

                if (msg == null || _handler == null) continue;

                // Pass Msg to Adapter
                _handler.HandleMessage(msg);
            }

            //Thread finished its job, clean up.
            _messages.Clear();
            _thread = null;
            _handler = null;
            _messages = null;
            _dataLock = null;
            _threadLock = null;
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            if (ExceptionDelegate != null)
                ExceptionDelegate(Name, e);
            else
                throw e;
        }
    }

    private void ResumeThread()
    {
        if (abort) return;
        _pause = false;
        lock (_threadLock)
        {
            Monitor.Pulse(_threadLock);
        }
    }

    public void SendMessage(Message message)
    {
        if (abort) return;
        lock (_dataLock)
        {
            _messages.Add(message);
            ResumeThread();
        }
    }

    public void SendMessageAtFrontOfQueue(Message message)
    {
        if (abort) return;
        lock (_dataLock)
        {
            _messages.Insert(0, message);
            ResumeThread();
        }
    }

    public void SendEmptyMessage(int What)
    {
        if (abort) return;
        lock (_dataLock)
        {
            _messages.Add(new Message(What));
            ResumeThread();
        }
    }

    public void SendEmptyMessageDelayed(int What, long DelayMilliseconds)
    {
        SendMessageDelayed(new Message(What), DelayMilliseconds);
    }

    private class TimerObject
    {
        public Timer timer;
        public Message message { get; protected set; }

        public TimerObject(Message message)
        {
            this.message = message;
        }
    }

    public void SendMessageDelayed(Message message, long DelayMilliseconds)
    {
        if (abort) return;
        if (DelayMilliseconds == 0)
        {
            SendMessage(message);
            return;
        }

        try
        {
            TimerObject obj = new TimerObject(message);
            Timer timer = new Timer(x =>
            {
                try
                {
                    TimerObject timerObject = (TimerObject)x;
                    lock (_dataLock)
                    {
                        SendMessageAtFrontOfQueue(timerObject.message);
                        if (_timers == null) return;
                        _timers.Remove(timerObject.timer);
                    }
                    timerObject.timer.Dispose();
                }
                catch (Exception e)
                {
                    if (ExceptionDelegate != null)
                        ExceptionDelegate(Name, e);
                    else
                        throw e;
                }
            },
                obj,
                (int)DelayMilliseconds,
                Timeout.Infinite
                );
            obj.timer = timer;

            lock (_dataLock)
            {
                _timers.Add(timer);
            }
        }
        catch (Exception e)
        {
            if (ExceptionDelegate != null)
                ExceptionDelegate(Name, e);
            else
                throw e;
        }
    }

    public void RemoveMessages(int What)
    {
        if (abort) return;
        lock (_dataLock)
        {
            for (int i = 0; i < _messages.Count; i++)
                if (_messages[i].What == What)
                    _messages.RemoveAt(i--);
        }
    }

    public void RemoveAllMessages()
    {
        if (abort) return;
        lock (_dataLock)
        {
            foreach (Timer t in _timers) t.Dispose();
            _timers.Clear();
            _messages.Clear();
        }
    }

    public void QuitSafely()
    {
        if (abort) return;
        abort = true;
        ResumeThread();
    }
}
#endif