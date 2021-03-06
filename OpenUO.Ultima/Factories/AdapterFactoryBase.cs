﻿#region License Header

// Copyright (c) 2015 OpenUO Software Team.
// All Right Reserved.
// 
// AdapterFactoryBase.cs
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

#endregion

#region Usings

using System;
using System.Threading.Tasks;

using OpenUO.Core;
using OpenUO.Core.Patterns;
using OpenUO.Core.Threading.Tasks;
using OpenUO.Ultima.Adapters;

#endregion

namespace OpenUO.Ultima
{
    public abstract class AdapterFactoryBase : IDisposable
    {
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly IContainer _container;
        private readonly InstallLocation _install;
        private IStorageAdapter _adapter;

        protected AdapterFactoryBase(InstallLocation install, IContainer container)
        {
            Guard.RequireIsNotNull(install, "install");
            Guard.RequireIsNotNull(container, "container");

            _container = container;
            _install = install;
        }

        public void Dispose()
        {
            if (_adapter != null)
            {
                _adapter.Dispose();
                _adapter = null;
            }

            Dispose(true);
        }

        protected TStorageAdapter GetAdapter<TStorageAdapter>()
            where TStorageAdapter : class, IStorageAdapter
        {
            if (_adapter == null)
            {
                _adapter = _container.Resolve<TStorageAdapter>();
                _adapter.Install = _install;
                _adapter.Initialize();
            }

            return (TStorageAdapter) _adapter;
        }

        protected async Task<TStorageAdapter> GetAdapterAsync<TStorageAdapter>()
            where TStorageAdapter : class, IStorageAdapter
        {
            if (_adapter != null)
            {
                return (TStorageAdapter) _adapter;
            }

            using (await _asyncLock.LockAsync())
            {
                if (_adapter != null)
                {
                    return (TStorageAdapter) _adapter;
                }

                _adapter = _container.Resolve<TStorageAdapter>();
                _adapter.Install = _install;

                await _adapter.InitializeAsync().ConfigureAwait(false);
            }

            return (TStorageAdapter) _adapter;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}