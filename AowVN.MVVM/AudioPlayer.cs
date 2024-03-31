using ManagedBass;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AowVN.MVVM
{
    public static class AudioPlayer
    {
        private static int streamId = 0;
        public static void Play(string NameBGM, long loop = 0)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), NameBGM);
            using (Stream stream = AowVN.MVVM.Resource.BGM)
            {
                using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }

            if (streamId != 0)
            {
                Bass.ChannelStop(streamId);
            }
            streamId = Bass.CreateStream(tempFilePath, Flags: BassFlags.Loop);
            Bass.ChannelSetAttribute(streamId, ChannelAttribute.Volume, 0.4f);
            File.Delete(NameBGM);
            if (streamId != 0)
            {
                Bass.ChannelSetPosition(streamId, loop, (PositionFlags)17);
                Resume();
            }

        }

        public static void Resume()
        {
            Bass.ChannelPlay(streamId);
        }
        public static void Pause()
        {
            Bass.ChannelPause(streamId);
        }
        public static async void Stop(string NameBGM)
        {
                for (float i = 0.4f; i > 0; i -= 0.02f)
                {
                    Bass.ChannelSetAttribute(streamId, ChannelAttribute.Volume, i);
                    await Task.Delay(16);
                }
            Bass.StreamFree(streamId);
            File.Delete(Path.Combine(Path.GetTempPath(), NameBGM));
        }

    }
}
