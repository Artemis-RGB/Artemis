using System;
using System.ComponentModel;
using System.Threading;

namespace Artemis.Utilities
{
    /// <summary>
    ///     A value that only changes it's not changed again within a set time
    /// </summary>
    public class StickyValue<T> : IDisposable
    {
        private readonly int _stickyTime;
        private readonly BackgroundWorker _updateWorker;
        private T _toStick;
        private T _value;
        private int _waitTime;

        public StickyValue(int stickyTime)
        {
            _stickyTime = stickyTime;

            _updateWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            _updateWorker.DoWork += UpdateWorkerOnDoWork;
            _updateWorker.RunWorkerAsync();
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (_toStick.Equals(value))
                    return;

                _waitTime = _stickyTime;
                _toStick = value;
            }
        }

        public void Dispose()
        {
            _updateWorker.CancelAsync();
        }

        private void UpdateWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (!_updateWorker.CancellationPending)
            {
                if (_waitTime < _stickyTime)
                {
                    Thread.Sleep(10);
                    continue;
                }

                while (_waitTime > 0)
                {
                    Thread.Sleep(10);
                    _waitTime -= 10;
                }
                _value = _toStick;
            }
        }
    }
}