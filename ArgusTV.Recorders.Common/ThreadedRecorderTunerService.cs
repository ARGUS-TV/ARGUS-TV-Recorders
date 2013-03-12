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
using System.Text;
using System.Diagnostics;
using System.Globalization;

using ArgusTV.DataContracts;

namespace ArgusTV.Recorders.Common
{
	public abstract class ThreadedRecorderTunerService<T> : RecorderTunerServiceBase, IDisposable
		where T : RecordingThreadBase
	{
		#region Protected Members

		private bool _disposed;

        protected bool Disposed
        {
            get { return _disposed; }
        }

        private RecordingThreadCollection<T> _recordingThreads = new RecordingThreadCollection<T>();

        protected RecordingThreadCollection<T> RecordingThreads
        {
            get { return _recordingThreads; }
        }

		#endregion

        protected abstract void OnWriteLog(TraceEventType severity, string message);

        protected void Log(TraceEventType severity, string message, params object[] args)
        {
            OnWriteLog(severity, String.Format(CultureInfo.CurrentCulture, message, args));
        }

        protected void Log(string message, params object[] args)
        {
            OnWriteLog(TraceEventType.Information, String.Format(CultureInfo.CurrentCulture, message, args));
        }

        public override int Ping()
        {
            Log("{0} - Ping", this.Name);
            return base.Ping();
        }

        public override void Initialize(Guid recorderTunerId, string serverHostName, int tcpPort)
        {
            try
            {
                Log("{0} - Initialize from {1}:{2}", this.Name, serverHostName, tcpPort);
                base.Initialize(recorderTunerId, serverHostName, tcpPort);
            }
            catch (Exception ex)
            {
                Log(TraceEventType.Error, "{0} - Initialization error: {1}", this.Name, ex.Message);
            }
        }

        public override bool ValidateAndUpdateRecording(CardChannelAllocation channelAllocation, UpcomingProgram recordingProgram, DateTime stopTimeUtc)
		{
			bool threadNotFound;
			bool result = _recordingThreads.ValidateAndUpdateRecording(recordingProgram, stopTimeUtc, out threadNotFound);
			if (threadNotFound)
			{
				Log(TraceEventType.Warning, "{0} - ValidateAndUpdateRecording called on unknown recording {1}", this.Name, recordingProgram.CreateProgramTitle());
			}
			return result;
		}

		public override bool AbortRecording(string serverHostName, int tcpPort, UpcomingProgram recordingProgram)
		{
			Log("{0} - Abort recording {1}", this.Name, recordingProgram.CreateProgramTitle());
			if (!_recordingThreads.StopThreadAsync(recordingProgram))
			{
				Log(TraceEventType.Warning, "{0} - Recording {1} not found, failed to abort", this.Name, recordingProgram.CreateProgramTitle());
                return false;
			}
            return true;
		}

		#region IDisposable Pattern

		~ThreadedRecorderTunerService()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Managed resources.
					foreach (RecordingThreadBase recordingThread in _recordingThreads.Values)
					{
                        recordingThread.Stop(false);
					}
                    foreach (RecordingThreadBase recordingThread in _recordingThreads.Values)
                    {
                        if (!recordingThread.Join(5000))
                        {
                            recordingThread.Abort();
                        }
                    }
                    _recordingThreads.Clear();
				}
				// We have no unmanaged resources.
			}
			_disposed = true;
        }
        #endregion
    }
}
