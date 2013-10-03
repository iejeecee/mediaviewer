using MvvmFoundation.Wpf;
using SharpDX.DirectSound;
using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MediaViewer.VideoPanel
{
    class AudioPlayer
    { 
        const int DSBVOLUME_MIN = -10000;
        const int DSBVOLUME_MAX = 0;

        public enum AudioState
        {
            START_PLAY_AFTER_NEXT_WRITE,
            PLAYING,
            STOPPED
        }

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DirectSound directSound;
        SecondarySoundBuffer audioBuffer;
        int offsetBytes;
        int bufferSizeBytes;

        int bytesPerSample;
        int samplesPerSecond;
        int nrChannels;

        double volume;
        bool isMuted;
        AudioState audioState;
        
        byte[] silence;

        void releaseResources()
        {
            if (audioBuffer != null)
            {
                Disposer.RemoveAndDispose<SecondarySoundBuffer>(ref audioBuffer);          
            }
        }

        double pts;
        int ptsPos;
        int prevPtsPos;
        int prevPlayPos;
        int playLoops;
        int ptsLoops;

        public AudioPlayer()
        {
            directSound = null;        

            audioBuffer = null;
            volume = 1;
            isMuted = false;

            pts = 0;
            offsetBytes = 0;
            ptsPos = 0;
            prevPtsPos = 0;
            playLoops = 0;
            ptsLoops = 0;
        }

        ~AudioPlayer()
        {

            releaseResources();

            if (directSound != null)
            {
                Disposer.RemoveAndDispose<DirectSound>(ref directSound);             
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

        public bool IsMuted
        {

            set
            {
                this.isMuted = value;
                Volume = volume;
               
            }

            get
            {
                return (isMuted);
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
                volume = value;

                if (audioBuffer != null && isMuted == false)
                {
                
                    //audioBuffer.Volume = (int)Utils.Misc.lerp(volume, MinVolume, MaxVolume);
                    audioBuffer.Volume = (int)value;

                }
                else if (audioBuffer != null && isMuted == true)
                {

                    audioBuffer.Volume = DSBVOLUME_MIN;
                }

               
            }

            get
            {
                return (volume);
            }

        }

        public int MinVolume
        {
            get
            {
                return (DSBVOLUME_MIN - DSBVOLUME_MIN / 3);
            }
        }

        public int MaxVolume
        {
            get
            {
                return (DSBVOLUME_MAX);
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
                    IntPtr hwnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;

                    directSound.SetCooperativeLevel(hwnd, CooperativeLevel.Priority);
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

                Volume = MaxVolume;
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

        public double getAudioClock()
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

        public void write(VideoLib.AudioFrame frame)
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
