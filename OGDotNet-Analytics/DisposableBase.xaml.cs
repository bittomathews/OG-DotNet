﻿using System;

namespace OGDotNet_Analytics
{
    public abstract class DisposableBase : IDisposable
    {
        ~DisposableBase()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }
}