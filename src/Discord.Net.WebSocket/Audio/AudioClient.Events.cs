using System;
using System.Threading.Tasks;

namespace Discord.Audio
{
    internal partial class AudioClient
    {
        public event EventHandler Connected;

        public event EventHandler<Exception> Disconnected;

        public event EventHandler<LatencyUpdatedArguments> LatencyUpdated;

        public event EventHandler<LatencyUpdatedArguments> UdpLatencyUpdated;

        public event EventHandler<StreamCreatedArguments> StreamCreated;

        public event EventHandler<ulong> StreamDestroyed;

        public event EventHandler<SpeakingUpdatedArguments> SpeakingUpdated;
    }
}
