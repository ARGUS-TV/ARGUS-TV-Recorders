/*
 *	Copyright (C) 2007-2012 ARGUS TV
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
using System.Globalization;
using System.Net.Security;
using System.ServiceModel;
using System.Text;
using System.Xml;

using ArgusTV.ServiceContracts;

namespace ArgusTV.Recorders.Common
{
    public class RecorderTunerServiceHost : ServiceHost
    {
        public RecorderTunerServiceHost(Type serviceType, int tcpPort)
            : base(serviceType, GetServiceUri(tcpPort))
        {
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas()
            {
                MaxStringContentLength = int.MaxValue,
                MaxDepth = int.MaxValue,
                MaxArrayLength = int.MaxValue,
                MaxNameTableCharCount = int.MaxValue,
                MaxBytesPerRead = int.MaxValue
            };

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            binding.ReceiveTimeout = new TimeSpan(0, 30, 0);
            binding.SendTimeout = new TimeSpan(0, 30, 0);
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.MaxReceivedMessageSize = 256 * 1024 * 1024;
            binding.ReaderQuotas = quotas;
            binding.Namespace = "http://www.argus-tv.com";
            this.AddServiceEndpoint(typeof(IRecorderTunerService), binding, "RecorderTunerService");
        }

        private static Uri GetServiceUri(int tcpPort)
        {
            return new Uri(String.Format(CultureInfo.InvariantCulture, "net.tcp://localhost:{0}/", tcpPort));
        }
    }
}
