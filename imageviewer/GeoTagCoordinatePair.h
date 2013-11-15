#pragma once

namespace imageviewer {

using namespace System;
using namespace System::Globalization;

public ref class GeoTagCoordinate
{
private:

	int degrees;
	int minutes;
	int seconds;
	int secondsFraction;
	System::Char direction;

	double decimal;

	bool isLat;

public:

	GeoTagCoordinate(bool isLat) {

		this->isLat = isLat;
	}

	property String ^Coord {

		void set(String ^coord) {

			degrees = 0;
			minutes = 0;
			seconds = 0;
			secondsFraction = 0;
			direction = '0';
			decimal = 0;

			int s1 = coord->IndexOf(",");
			int s2 = coord->LastIndexOf(",");
			int s3 = coord->IndexOf(".");

			int s4 = (s2 == -1 || s1 == s2) ? s3 : s2;

			degrees = Convert::ToInt32(coord->Substring(0, s1));
			minutes = Convert::ToInt32(coord->Substring(s1 + 1, s4 - s1 - 1));

			int fractLength = coord->Length - s4 - 2;
			int temp = Convert::ToInt32(coord->Substring(s4 + 1, fractLength));

			if(s2 == -1 || s1 == s2) {

				secondsFraction = temp;

				double d = Math::Pow(10, fractLength);

				seconds = int((secondsFraction / d) * 60);	

				decimal = degrees + ((minutes + secondsFraction / d) / 60);

			} else {

				seconds = temp;
				secondsFraction = 0;

				decimal = degrees + (minutes / 60.0) + (seconds / 3600.0);
			}

			direction = Char::ToUpper(coord[coord->Length - 1]);

			if(direction == 'W' || direction == 'S') {

				decimal *= -1;
			}

		}

		String ^get() {

			String ^result = String::Format("{0},{1}.{2}{3}",
				Convert::ToString(degrees),
				Convert::ToString(minutes),
				Convert::ToString(secondsFraction),
				direction);

			return(result);
		}
	}

	property bool IsLat {

		bool get() {

			return(isLat);
		}
	}

	property double Decimal {

		double get() {

			return(decimal);
		}

		void set(double decimal) {

			this->decimal = decimal;

			degrees = (int)Math::Truncate(Math::Abs(decimal));
			
			minutes = ((int)Math::Truncate(Math::Abs(decimal) * 60)) % 60;

			double fract = (Math::Abs(decimal) * 3600) / 60;
			fract = fract - Math::Floor(fract);

			seconds = (int)(fract * 60);
			secondsFraction = (int)(fract * 10000);
			
			if(decimal < 0) {

				if(isLat) {

					direction = 'S';

				} else {

					direction = 'W';
				}

			} else {

				if(isLat) {

					direction = 'N';

				} else {

					direction = 'E';
				}
			} 

		}
	}

};

public ref class GeoTagCoordinatePair {

public:

	GeoTagCoordinate ^latitude;
	GeoTagCoordinate ^longitude;

	GeoTagCoordinatePair() {

		latitude = gcnew GeoTagCoordinate(true);
		longitude = gcnew GeoTagCoordinate(false);
	}

};

}