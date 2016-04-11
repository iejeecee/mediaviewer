#pragma once
#include "..\..\Video\VideoDecoderFactory.h"
#include "..\..\Frame\VideoFrame.h"
#include "Option.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Threading;

namespace VideoLib2
{
	public ref class MediaFilter
	{
	public:
			
		property String ^FilterName {

			String ^get() 
			{
				return filterName;
			}

		protected:
			void set(String ^value)
			{
				filterName = value;
			}
		}
		
		property String ^InstanceName {

			String ^get() 
			{
				return filterName;
			}

		protected:
			void set(String ^value)
			{
				filterName = value;
			}
		}

		property int NrInputs {

			int get()
			{
				return filterContext->nb_inputs;				
			}

		}

		property int NrOutputs {

			int get()
			{
				return filterContext->nb_outputs;
			}

		}
		
	protected:

		FilterGraph *filterGraph;
		AVFilter *filter;
		AVFilterContext *filterContext;

		String ^filterName;
		String ^instanceName;
		Dictionary<String ^, Option ^> ^filterOpts;							
										
		MediaFilter(String ^filterName, String ^instanceName, IntPtr filterGraph) {
			
			this->filterGraph = (FilterGraph *)filterGraph.ToPointer();
			FilterName = filterName;
			InstanceName = instanceName;
				
			filter = avfilter_get_by_name(marshal_as<std::string>(filterName).c_str());
			if (filter == NULL) {

				throw gcnew VideoLib2::VideoLibException(filterName + " filter not found");					
			}			
		
			filterContext = avfilter_graph_alloc_filter(this->filterGraph->getAVFilterGraph(), filter, marshal_as<std::string>(instanceName).c_str());
			if (filterContext == NULL) {

				throw gcnew VideoLib2::VideoLibException("Error allocating " + filterName + " filter");					
			}
						
		}

		void linkToOutput(int outputIndex, MediaFilter ^filter, int inputIndex)
		{
			
			int result = avfilter_link(filterContext, outputIndex, filter->filterContext, inputIndex);
			if(result != 0) {

				throw gcnew VideoLib2::VideoLibException("Error linking filter " + InstanceName + " with " + filter->InstanceName, result);
			}
		}

		void initFilter() {

			int result = avfilter_init_str(filterContext, NULL);
			if (result < 0) {

				throw gcnew VideoLib2::VideoLibException("Error initializing filter: " + InstanceName);	
			}

		}
		
	};

}