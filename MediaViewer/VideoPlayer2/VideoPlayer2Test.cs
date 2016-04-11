using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLib2;

namespace MediaViewer.VideoPlayer2
{
    class VideoPlayer2Test
    {
        public void Test()
        {
            VideoPlayer2 mediaTransformer = new VideoPlayer2();
            OpenVideoArgs openArgs = new OpenVideoArgs("d:\\b (1).mp4");

            mediaTransformer.enableLibAVLogging(MediaTransformer.LogLevel.LOG_LEVEL_DEBUG);
            mediaTransformer.setLogCallback(logCallback);

            MediaFileSource video = new MediaFileSource(openArgs, "input", mediaTransformer.getFilterGraph(), 0, Double.MaxValue, 1);

            mediaTransformer.addInput(video);

            MediaFileSink output = new MediaFileSink("d:\\masseffect.mp4", "output", mediaTransformer.getFilterGraph(), "libx264",
                1280, 720, 30, null, 48000, 2,null);//"libvo_aacenc",48000,2);

            mediaTransformer.addOutput(output);           

            mediaTransformer.start();

        }

        void logCallback(int level, String message)
        {
            System.Diagnostics.Debug.Print(message);
        }
    }
}
