/*
 *	Copyright (C) 2007-2014 ARGUS TV
 *	http://www.argus-tv.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Globalization;
using System.Net.Security;

using ArgusTV.DataContracts;

namespace ArgusTV.Recorders.Common
{
    /// <summary>
    /// INTERNAL USE ONLY.
    /// </summary>
    /// <typeparam name="T">The service interface.</typeparam>
    public class NamedPipeAgent<T> : MarshalByRefObject, IDisposable
        where T : class
    {
        private static ChannelFactory<T> _factory;
        private T _proxy;

        private object _syncLock = new object();

        /// <summary>
        /// INTERNAL USE ONLY.
        /// </summary>
        public NamedPipeAgent()
        {
        }

        /// <summary>
        /// INTERNAL USE ONLY.
        /// </summary>
        public T Proxy
        {
            get
            {
                EnsureProxy();
                return _proxy;
            }
        }

        private void DisposeProxy()
        {
            if (_proxy != null)
            {
                lock (_syncLock)
                {
                    if (_proxy != null)
                    {
                        ICommunicationObject wcfProxy = Proxy as ICommunicationObject;
                        try
                        {
                            if (wcfProxy != null)
                            {
                                try
                                {
                                    if (wcfProxy.State != CommunicationState.Faulted)
                                    {
                                        wcfProxy.Close();
                                    }
                                    else
                                    {
                                        wcfProxy.Abort();
                                    }
                                }
                                catch (CommunicationException)
                                {
                                    wcfProxy.Abort();
                                }
                                catch (TimeoutException)
                                {
                                    wcfProxy.Abort();
                                }
                                catch
                                {
                                    wcfProxy.Abort();
                                    throw;
                                }
                            }
                        }
                        finally
                        {
                            _proxy = null;
                        }
                    }
                }
            }
        }

        private void EnsureProxy()
        {
            if (_proxy == null)
            {
                lock (_syncLock)
                {
                    if (_proxy == null)
                    {
                        if (_factory == null)
                        {
                            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
                            EndpointAddress endPoint = new EndpointAddress("net.pipe://localhost/ArgusTV/" + typeof(T).Name);
                            _factory = new ChannelFactory<T>(binding, endPoint);
                        }
                        _proxy = _factory.CreateChannel();
                    }
                }
            }
        }

        #region IDisposable Pattern

        private bool _disposed;
        
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeProxy();
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// Disposes the service agent.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NamedPipeAgent()
        {
            Dispose(false);
        }

        #endregion
    }
}
