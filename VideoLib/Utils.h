#pragma once
#include "Video.h"

namespace VideoLib {

	class Utils {

	private:

		static char *AVOptionTypeToString(AVOptionType type) {

			switch (type)	
			{
			case AV_OPT_TYPE_FLAGS:
				return("flags");
				break;
			case AV_OPT_TYPE_INT:
				return("int");
				break;
			case AV_OPT_TYPE_INT64:
				return("int64");
				break;
			case AV_OPT_TYPE_DOUBLE:
				return("double");
				break;
			case AV_OPT_TYPE_FLOAT:
				return("float");
				break;
			case AV_OPT_TYPE_STRING:
				return("string");
				break;
			case AV_OPT_TYPE_RATIONAL:
				return("rational");
				break;
			case AV_OPT_TYPE_BINARY:
				return("binary");
				break;
			case AV_OPT_TYPE_DICT:
				return("dict");
				break;
			case AV_OPT_TYPE_CONST:
				return("const");
				break;
			case AV_OPT_TYPE_IMAGE_SIZE:
				return("image size");
				break;
			case AV_OPT_TYPE_PIXEL_FMT:
				return("pixel fmt");
				break;
			case AV_OPT_TYPE_SAMPLE_FMT:
				return("sample fmt");
				break;
			case AV_OPT_TYPE_VIDEO_RATE:
				return("video rate");
				break;
			case AV_OPT_TYPE_DURATION:
				return("duration");
				break;
			case AV_OPT_TYPE_COLOR:
				return("color");
				break;
			case AV_OPT_TYPE_CHANNEL_LAYOUT:
				return("channel layout");
				break;
			case AV_OPT_TYPE_BOOL:
				return("bool");
				break;
			default:
				throw gcnew VideoLibException("Unknown AVOptionType");
				break;
			}

		}

		static void printObjectOpts(void *obj, char *objName, bool isChildOpt)
		{
			const AVOption *opt = NULL;

			char objNameFull[1024];

			if(objName == NULL) {

				strcpy(objNameFull, "object");			

			} else {

				strcpy(objNameFull, objName);
			}

			if(isChildOpt) {

				strcat(objNameFull, " (child)");
			}
			
			while((opt = av_opt_next(obj, opt)) != NULL) {

				char info[4096];
			
				_snprintf(info, sizeof(info),
					"%s: %s (%s) - %s", objNameFull, opt->name, AVOptionTypeToString(opt->type), opt->help);
			
				VideoInit::writeToLog(AV_LOG_INFO, info);
			}
		}

	public:

		static void printOpts(void *obj, char *objName = NULL, bool iterateChildren = true) {

			printObjectOpts(obj, objName, false);

			void *child = NULL;

			while(iterateChildren == true && (child = av_opt_child_next(obj, child)) != NULL) {

				printObjectOpts(child, objName, true);
			}
		}

		static void listRegisteredInputFormats() {

			AVInputFormat *input = NULL;

			while((input = av_iformat_next(input)) != NULL) {

				char info[4096];
			
				_snprintf(info, sizeof(info),
					"input format: %s - %s", input->name, input->long_name);

				VideoInit::writeToLog(AV_LOG_INFO, info);
			}

		}

		static void listRegisteredOutputFormats() {

			AVOutputFormat *output = NULL;

			while((output = av_oformat_next(output)) != NULL) {

				char info[4096];
			
				_snprintf(info, sizeof(info),
					"output format: %s - %s", output->name, output->long_name);

				VideoInit::writeToLog(AV_LOG_INFO, info);
			}

		}
	};

}