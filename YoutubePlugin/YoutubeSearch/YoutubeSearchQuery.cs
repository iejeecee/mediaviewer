using Google.Apis.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlugin.YoutubeSearch
{
    class YoutubeSearchQuery
    {
        public YoutubeSearchQuery(IClientServiceRequest request, String queryName = "")
        {
            Request = request;         
            QueryName = queryName;            
        }

        public IClientServiceRequest Request { get; set; }
        public String QueryName { get; set; }

    }
}
