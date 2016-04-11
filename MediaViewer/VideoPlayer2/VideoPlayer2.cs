using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLib2;

namespace MediaViewer.VideoPlayer2
{
    class VideoPlayer2 : VideoLib2.MediaTransformer
    {
        protected override void buildGraph()
        {
            System.Diagnostics.Debug.Print("buildgraph called");

            InOutMediaFilter videoNull = new InOutMediaFilter("null", "videoNull", getFilterGraph());
            addFilter(videoNull);

            //InOutMediaFilter audioNull = new InOutMediaFilter("anull", "audioNull", getFilterGraph());
            //addFilter(audioNull);

            foreach (OutMediaFilter sourceFilter in inputs[0].SourceFilter)
            {
                if (sourceFilter is VideoBufferSourceFilter)
                {
                    sourceFilter.link(0, videoNull, 0);
                    videoNull.link(0, outputs[0].SinkFilter[0], 0);
                }
                else if (sourceFilter is AudioBufferSourceFilter)
                {
                    //sourceFilter.link(0, audioNull, 0);
                    //audioNull.link(0, outputs[0].SinkFilter[1], 0);
                }

            }
            
            
        }
    }
}
