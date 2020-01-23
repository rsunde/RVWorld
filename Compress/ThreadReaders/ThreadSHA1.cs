﻿using System;
using System.Security.Cryptography;
using System.Threading;

namespace Compress.ThreadReaders
{
    public class ThreadSHA1 : IDisposable
    {
        private readonly AutoResetEvent _waitEvent;
        private readonly AutoResetEvent _outEvent;
        private readonly Thread _tWorker;

        private readonly SHA1 _sha1;

        private byte[] _buffer;
        private int _size;
        private bool _finished;

        public ThreadSHA1(bool threaded = true)
        {
            _sha1 = SHA1.Create();
            if (!threaded)
                return;

            _waitEvent = new AutoResetEvent(false);
            _outEvent = new AutoResetEvent(false);
            _finished = false;

            _tWorker = new Thread(MainLoop);
            _tWorker.Start();
        }

        public byte[] Hash => _sha1.Hash;

        public void Dispose()
        {
            _waitEvent?.Close();
            _outEvent?.Close();
            // _sha1.Dispose();
        }

        private void MainLoop()
        {
            while (true)
            {
                _waitEvent.WaitOne();
                if (_finished)
                {
                    break;
                }
                _sha1.TransformBlock(_buffer, 0, _size, null, 0);
                _outEvent.Set();
            }

            byte[] tmp = new byte[0];
            _sha1.TransformFinalBlock(tmp, 0, 0);
        }

        public void Trigger(byte[] buffer, int size)
        {
            if (_waitEvent != null)
            {
                _buffer = buffer;
                _size = size;
                _waitEvent.Set();
            }
            else
            {
                _sha1.TransformBlock(buffer, 0, size, null, 0);
            }
        }

        public void Wait()
        {
            _outEvent?.WaitOne();
        }

        public void Finish()
        {
            if (_waitEvent != null)
            {
                _finished = true;
                _waitEvent.Set();
                _tWorker.Join();
            }
            else
            {
                byte[] tmp = new byte[0];
                _sha1.TransformFinalBlock(tmp, 0, 0);
            }
        }
    }
}