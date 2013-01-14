#pragma once
#include "stdafx.h"

class ImageRGB24 {

private:

	int width;
	int height;

	// timestamp
	int seconds;
	
	unsigned char *data;
	bool externalData;

public:

	ImageRGB24(int width, int height, int seconds, unsigned char *imageData) {

		this->width = width;
		this->height = height;

		this->seconds = seconds;

		data = new unsigned char[getSizeBytes()];
		memcpy(data, imageData, getSizeBytes());

	}

	~ImageRGB24() {
		
		delete data;
		
	}

	int getWidth() const {

		return(width);
	}

	int getHeight() const {

		return(height);
	}

	int getTimeStampSeconds() const {

		return(seconds);
	}

	const unsigned char *getData() const {

		return(data);
	}

	unsigned char *getData() {

		return(data);
	}

	int getSizeBytes() const {

		return(width * height * 3);
	}

	void clear(unsigned char red = 0, unsigned char green = 0, unsigned char blue = 0) {

		for(int i = 0; i < getSizeBytes(); i += 3) {

			data[i] = red;
			data[i + 1] = green;
			data[i + 2] = blue;

		}

	}

};