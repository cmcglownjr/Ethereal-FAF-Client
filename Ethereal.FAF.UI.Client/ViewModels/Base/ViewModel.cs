﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ethereal.FAF.UI.Client.ViewModels.Base
{
    public class ViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private bool _Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _Disposed) return;
            _Disposed = true;
            // Clean resources
        }
    }
}
