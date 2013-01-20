#include "stdafx.h"
#include "VideoFrameGrabber.h"
#include "VideoTranscoder.h"
#include "jpeg.h"
#include <stdlib.h>
#include <exception>
// http://tclap.sourceforge.net/
// Note: tclap command line parser doesn't support unicode, so turn it off in project settings
#include <tclap/cmdLine.h>
#include "IntegerRangeConstraint.h"
#include "WindowsFileUtil.h"
#include "RenderText.h"
#include "SplitCommandLine.h"

int _tmain(int argc, char** argv)
{

	//std::string videoLocation("J:/Girls/Lucy Belle/Lucy Belle - Ass Traffic.wmv");
	//std::string videoLocation("J:/Girls/Mandy Dee/Mandy Dee - Buttman's Bend Over Babes #7.mp4");
	//std::string videoLocation("J:/Girls/Mandy Dee/Mandy Dee - From Russia With Lust.avi");	

	std::string videoLocation;
	std::string outputPath;
	int frameWidth;
	int nrRows;
	int nrColumns;
	int quality;
	int captureTime;
	bool saveFrames;
	bool forceOverwrite;

	try {

		TCLAP::CmdLine cmd("AVLIB TOOL, Frame Grabber", ' ', "0.1");

		TCLAP::ValueArg<std::string> inputArg("i", "input", "video input location", true, "", "location");
		
		IntegerRangeConstraint widthRange(32, 4096);
		TCLAP::ValueArg<int> widthArg("w", "width", "frame width (default 400)", false, 400, &widthRange);

		IntegerRangeConstraint gridRange(1, 256);
		TCLAP::ValueArg<int> nrRowsArg("r", "nrRows", "number of rows (default 32), overridden when -t is set", false, -1, &gridRange);
		TCLAP::ValueArg<int> nrColumnsArg("c", "nrColumns", "number of columns (default 2)", false, 2, &gridRange);

		IntegerRangeConstraint qualityRange(1, 100);
		TCLAP::ValueArg<int> qualityArg("q", "quality", "jpeg quality (default 90)", false, 90, &qualityRange);

		IntegerRangeConstraint timeRange(1, INT_MAX);
		TCLAP::ValueArg<int> timeArg("t", "time", "grab a frame every t seconds", false, -1, &timeRange);

		TCLAP::ValueArg<std::string> outputArg("o", "output", "output path with optional filename", false, "./", "location");
		TCLAP::SwitchArg saveSwitch("s","save","save intermediate frames seperately", false);

		TCLAP::SwitchArg forceSwitch("f","force","force overwriting existing output images", false);
		
		cmd.add(forceSwitch);
		cmd.add(saveSwitch);
		cmd.add(outputArg);
		cmd.add(widthArg);
		cmd.add(nrRowsArg);
		cmd.add(nrColumnsArg);
		cmd.add(qualityArg);
		cmd.add(timeArg);
		cmd.add(inputArg);
		
		LPSTR commandLine = GetCommandLine();

		SplitCommandLine cl(commandLine);

		cmd.parse( cl.getArgc(), cl.getArgv());

		videoLocation = inputArg.getValue();
		frameWidth = widthArg.getValue();
		nrRows = nrRowsArg.getValue();
		nrColumns = nrColumnsArg.getValue();
		quality = qualityArg.getValue();
		captureTime = timeArg.getValue();
		outputPath = outputArg.getValue();
		saveFrames = saveSwitch.getValue();
		forceOverwrite = forceSwitch.getValue();

	} catch (TCLAP::CmdLineParseException e) { 
				
		std::cerr << "error: " << e.error() << " for arg " 
			<< e.argId() << std::endl; 
		exit(1);
	}

	std::string drive;
	std::string dir;
	std::string videoName;
	std::string videoExtension;

	boolean isLocalLocation = WindowsFileUtil::splitPath(videoLocation, &drive, &dir, &videoName, &videoExtension);

	videoName += videoExtension;

	std::list<std::string> inputLocation;

	if(isLocalLocation && videoName.empty()) {

		// a local directory was specified as input

		if(videoLocation.size() > 0) {
			// remove trailing slash
			videoLocation.resize(videoLocation.size() - 1);
		}

		WindowsFileUtil::getAllFilesRecursively(inputLocation, videoLocation);
	
	} else if(isLocalLocation) {

		if(videoName.find("*") != std::string::npos ||
			videoName.find("?") != std::string::npos) 
		{
			// video name contains wildcard(s)

			if(dir.size() > 0) {
				// remove trailing slash
				dir.resize(dir.size() - 1);
			}

			WindowsFileUtil::getAllFiles(inputLocation, drive + dir, videoName);

		} else {

			inputLocation.push_back(videoLocation);
		}
		
		
	} else {

		inputLocation.push_back(videoLocation);
	}

	while(!inputLocation.empty()) {

		std::cout << "Input: " << inputLocation.front() << "\n";

		try {

			VideoFrameGrabber video;// = new VideoFrameGrabber();

			video.grab(inputLocation.front(), outputPath, frameWidth, nrRows, nrColumns,
				quality, captureTime, saveFrames, forceOverwrite);

		} catch(std::exception &e) {

			std::cout << e.what() << std::endl;

		} 

		inputLocation.pop_front();
	} 


/*
	std::string inputLocation("C:/game/avlibtool/test3.avi");
	outputPath = "C:/game/avlibtool/output.ts";

	VideoTranscoder *video = new VideoTranscoder();

	try {

		video->transcode(inputLocation, outputPath);

	} catch(std::exception &e) {

		std::cout << e.what() << std::endl;

	} 

	if(video != NULL) {

		delete video;
	}
*/

	return 0;
}

