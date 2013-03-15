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
using System.ServiceModel;
using System.Globalization;
using System.Reflection;

using ArgusTV.DataContracts;
using ArgusTV.DataContracts.Tuning;
using ArgusTV.ServiceAgents;
using ArgusTV.ServiceContracts;

namespace ArgusTV.Recorders.Common
{
    public abstract class RecorderTunerServiceBase : IRecorderTunerService
    {
        protected abstract string Name
        {
            get;
        }

        private Guid _recorderTunerId;

        protected Guid RecorderTunerId
        {
            get { return _recorderTunerId; }
        }

        /// <summary>
        /// Check if a channel was already allocated to a card.
        /// </summary>
        /// <param name="alreadyAllocated">The array of previously allocated cards.</param>
        /// <param name="cardId">The ID of the card we want to check.</param>
        /// <param name="channelId">The ID of the channel.</param>
        /// <returns>True if the channel was already allocated to this card, false otherwise.</returns>
        protected bool ChannelAlreadyAllocatedOn(CardChannelAllocation[] alreadyAllocated, string cardId, Guid channelId)
        {
            foreach (CardChannelAllocation allocation in alreadyAllocated)
            {
                if (allocation.CardId == cardId
                    && allocation.ChannelId == channelId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Count the number of times a card has been allocated.
        /// </summary>
        /// <param name="alreadyAllocated">The array of previously allocated cards.</param>
        /// <param name="cardId">The ID of the card we want to check.</param>
        /// <returns>The number of times this card has been allocated.</returns>
        protected int CountNumTimesAllocated(CardChannelAllocation[] alreadyAllocated, string cardId)
        {
            int count = 0;
            foreach (CardChannelAllocation allocation in alreadyAllocated)
            {
                if (allocation.CardId == cardId)
                {
                    count++;
                }
            }
            return count;
        }

        #region IRecorderTunerService Members

        public virtual int Ping()
        {
            return Constants.RecorderApiVersion;
        }

        public virtual void Initialize(Guid recorderTunerId, string serverHostName, int tcpPort)
        {
            _recorderTunerId = recorderTunerId;
            using (RecorderCallbackServiceAgent agent = new RecorderCallbackServiceAgent(serverHostName, tcpPort))
            {
                agent.RegisterRecorderTuner(this.RecorderTunerId, this.Name, Assembly.GetCallingAssembly().GetName().Version.ToString());
            }
        }

        public abstract string AllocateCard(Channel channel, ArgusTV.DataContracts.CardChannelAllocation[] alreadyAllocated, bool useReversePriority);

        public abstract bool StartRecording(string serverHostName, int tcpPort, CardChannelAllocation channelAllocation, DateTime startTimeUtc, DateTime stopTimeUtc, UpcomingProgram recordingProgram, string suggestedBaseFileName);

        public abstract bool ValidateAndUpdateRecording(CardChannelAllocation channelAllocation, UpcomingProgram recordingProgram, DateTime stopTimeUtc);

        public abstract bool AbortRecording(string serverHostName, int tcpPort, UpcomingProgram recordingProgram);

        public abstract string[] GetRecordingShares();

        public abstract string[] GetTimeshiftShares();

        #region Live TV/Radio

        public virtual LiveStreamResult TuneLiveStream(Channel channel, CardChannelAllocation upcomingRecordingAllocation, ref LiveStream liveStream)
        {
            liveStream = null;
            return LiveStreamResult.NotSupported;
        }

        public virtual void StopLiveStream(LiveStream liveStream)
        {
        }

        public virtual LiveStream[] GetLiveStreams()
        {
            return new LiveStream[] { };
        }

        public virtual bool KeepLiveStreamAlive(LiveStream liveTvStream)
        {
            return false;
        }

        public virtual ChannelLiveState[] GetChannelsLiveState(Channel[] channels, LiveStream liveStream)
        {
            return null;
        }

        public virtual ServiceTuning GetLiveStreamTuningDetails(LiveStream liveStream)
        {
            return null;
        }

        #endregion

        #region TeleText

        public virtual bool HasTeletext(LiveStream liveStream)
        {
            return false;
        }

        public virtual void StartGrabbingTeletext(LiveStream liveStream)
        {         
        }

        public virtual void StopGrabbingTeletext(LiveStream liveStream)
        { 
        }

        public virtual bool IsGrabbingTeletext(LiveStream liveStream)
        {
            return false;
        }

        public virtual byte[] GetTeletextPageBytes(LiveStream liveStream, int pageNumber, int subPageNumber, out int subPageCount)
        {
            subPageCount = 0;
            return null;
        }

        #endregion

        #endregion
    }
}
