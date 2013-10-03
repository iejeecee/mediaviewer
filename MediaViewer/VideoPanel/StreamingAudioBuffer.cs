//http://msdn.microsoft.com/en-us/library/windows/desktop/ms804968.aspx

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SharpDX.DirectSound;
using SharpDX.Multimedia;
using System.Windows;

namespace MediaViewer.VideoPanel
{

    public class StreamingAudioBuffer
    {
        static const int DSBVOLUME_MIN = -10000;
        static const int DSBVOLUME_MAX = 0;

        public enum AudioState
        {
            START_PLAY_AFTER_NEXT_WRITE,
            PLAYING,
            STOPPED
        }

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Windows.Forms.Control owner;

        DirectSound directSound;
        SecondarySoundBuffer audioBuffer;
        int offsetBytes;
        int bufferSizeBytes;

        int bytesPerSample;
        int samplesPerSecond;
        int nrChannels;

        double volume;
        bool muted;
        AudioState audioState;

        AutoResetEvent[] bufferEvents;

        byte[] silence;

        void releaseResources()
        {

            if (audioBuffer != null)
            {

                delete audioBuffer;
                audioBuffer = null;
            }
        }

        double pts;
        int ptsPos;
        int prevPtsPos;
        int prevPlayPos;
        int playLoops;
        int ptsLoops;

        public StreamingAudioBuffer(Windows.Forms.Control owner)
        {
            directSound = null;
            this.owner = owner;

            audioBuffer = null;
            volume = 1;
            muted = false;

            pts = 0;
            offsetBytes = 0;
            ptsPos = 0;
            prevPtsPos = 0;
            playLoops = 0;
            ptsLoops = 0;
        }

        ~StreamingAudioBuffer()
        {

            releaseResources();

            if (directSound != null)
            {

                delete directSound;
            }
        }

        public void startPlayAfterNextWrite()
        {

            audioState = AudioState.START_PLAY_AFTER_NEXT_WRITE;
        }

        public void flush()
        {

            if (audioBuffer != null)
            {

                audioBuffer.Stop();
                audioBuffer.CurrentPosition = 0;
                audioBuffer.Write(silence, 0, LockFlags.None);
            }

            offsetBytes = 0;
            prevPlayPos = 0;

            ptsPos = 0;
            prevPtsPos = 0;
            playLoops = 0;
            ptsLoops = 0;

            audioState = AudioState.START_PLAY_AFTER_NEXT_WRITE;
        }

        public void stop()
        {

            if (audioBuffer != null)
            {
                audioBuffer.Stop();
            }

            audioState = AudioState.STOPPED;
        }

        public bool Muted
        {

            set
            {

                this.muted = value;
                Volume = volume;

            }

            get
            {

                return (muted);
            }
        }

        public int SamplesPerSecond
        {

            set
            {

                audioBuffer.Frequency = value;
            }

            get
            {

                return (audioBuffer.Frequency);
            }
        }

        public double Volume
        {

            set
            {

                this.volume = value;

                if (audioBuffer != null && muted == false)
                {

                    int min = DSBVOLUME_MIN - DSBVOLUME_MIN / 3;

                    audioBuffer.Volume = (int)Utils.Misc.lerp(volume, min, DSBVOLUME_MAX); ;

                }
                else if (audioBuffer != null && muted == true)
                {

                    audioBuffer.Volume = DSBVOLUME_MIN;
                }
            }

            get
            {

                return (volume);
            }

        }

        public void initialize(int samplesPerSecond, int bytesPerSample, int nrChannels,
            int bufferSizeBytes)
        {

            try
            {

                if (directSound == null)
                {

                    directSound = new DirectSound();
                    directSound.SetCooperativeLevel(owner.Handle, CooperativeLevel.Priority);
                }

                releaseResources();

                this.bufferSizeBytes = bufferSizeBytes;
                this.bytesPerSample = bytesPerSample;
                this.samplesPerSecond = samplesPerSecond;
                this.nrChannels = nrChannels;

                SoundBufferDescription desc = new SoundBufferDescription();
                desc.BufferBytes = bufferSizeBytes;
                desc.Flags = BufferFlags.Defer | BufferFlags.GlobalFocus |
                    BufferFlags.ControlVolume | BufferFlags.ControlFrequency |
                    BufferFlags.GetCurrentPosition2;

                //desc.AlgorithmFor3D = Guid.Empty;

                int blockAlign = nrChannels * bytesPerSample;
                int averageBytesPerSecond = samplesPerSecond * blockAlign;

                WaveFormat format = WaveFormat.CreateCustomFormat(WaveFormatEncoding.Pcm,
                    samplesPerSecond, nrChannels, averageBytesPerSecond, blockAlign, bytesPerSample * 8);

                desc.Format = format;
                //desc.Format.Encoding = WaveFormatEncoding.Pcm;
                /*
                            desc.Format.SampleRate = samplesPerSecond;
                            desc.Format.BitsPerSample = bytesPerSample * 8;
                            desc.Format.Channels = nrChannels;
                            desc.Format.FormatTag = WaveFormatTag.Pcm;
                            desc.Format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));
                            desc.Format.AverageBytesPerSecond = format.SamplesPerSecond * format.BlockAlign;
                */

                /*desc.DeferLocation = true;
                desc.GlobalFocus = true;
                desc.ControlVolume = true;
                desc.CanGetCurrentPosition = true;
                desc.ControlFrequency = true;*/

                silence = new byte[bufferSizeBytes];
                Array.Clear(silence, 0, silence.Length);

                audioBuffer = new SecondarySoundBuffer(directSound, desc);

                Volume = volume;
                offsetBytes = 0;
                prevPlayPos = 0;
                ptsPos = 0;
                prevPtsPos = 0;
                playLoops = 0;
                ptsLoops = 0;

                log.Info("Direct Sound Initialized");

            }
            catch (SharpDX.SharpDXException e)
            {
                log.Error("Error initializing Direct Sound", e);
                MessageBox.Show("Error initializing Direct Sound: " + e.Message, "Direct Sound Error");
            }
            catch (Exception e)
            {
                log.Error("Error initializing Direct Sound", e);
            }
        }

        double getAudioClock()
        {

            // audioclock is: pts of last frame plus the
            // difference between playpos and the write position of the last frame in bytes
            // divided by bytespersecond.
            if (audioBuffer == null) return (0);

            int playPos, writePos;

            audioBuffer.GetCurrentPosition(out playPos, out writePos);

            if (ptsPos < prevPtsPos)
            {

                ptsLoops++;
                //Util.DebugOut("ptsLoops" + ptsLoops.ToString());
            }

            if (playPos < prevPlayPos)
            {

                playLoops++;
                //Util.DebugOut("playLoops" + playLoops.ToString());
            }

            Int64 totalPlayPos = bufferSizeBytes * playLoops + playPos;
            Int64 totalPtsPos = bufferSizeBytes * ptsLoops + ptsPos;

            int bytesPerSecond = samplesPerSecond * bytesPerSample * nrChannels;

            double seconds = (totalPlayPos - totalPtsPos) / (double)bytesPerSecond;

            double time = pts + seconds;

            prevPlayPos = playPos;
            prevPtsPos = ptsPos;

            return (time);
        }

        void write(VideoLib.AudioFrame frame)
        {

            if (audioBuffer == null || frame.Length == 0) return;

            // store pts for this frame and the byte offset at which this frame is
            // written
            pts = frame.Pts;
            ptsPos = offsetBytes;

            int playPos, writePos;
            audioBuffer.GetCurrentPosition(out playPos, out writePos);

            if (playPos <= offsetBytes && offsetBytes < writePos)
            {

                log.Warn("playpos:" + playPos.ToString() + " offset:" + offsetBytes.ToString() + " writePos:" + writePos.ToString() + " dataSize:" + frame.Length.ToString());
                offsetBytes = writePos;
            }

            audioBuffer.Write(frame.Data, 0, frame.Length, offsetBytes, LockFlags.None);

            offsetBytes = (offsetBytes + frame.Length) % bufferSizeBytes;

            if (audioState == AudioState.START_PLAY_AFTER_NEXT_WRITE)
            {

                audioBuffer.Play(0, PlayFlags.Looping);
                audioState = AudioState.PLAYING;
            }

        }

    }

}