#pragma once

using namespace System;

namespace imageviewer {

public ref class AuthInfo {

	private:

		String ^_accessToken;
		String ^_refreshToken;
		DateTime _timeOut;
		bool _success;

	public:

		AuthInfo() {

			_success = false;
		}

		AuthInfo(String ^accessToken, String ^refreshToken, int timeOut) {

			_accessToken = accessToken;
			_refreshToken = refreshToken;

			_timeOut = DateTime::Now.AddSeconds(timeOut);

			_success = true;
		}

		bool isActive() {

			if(DateTime::Now > timeOut) return(false);
			else return(true);
		}

		property String ^accessToken {

			String ^get() 
			{
				return(_accessToken);
			}
		}

		property String ^refreshToken {

			String ^get() 
			{
				return(_refreshToken);
			}
		}

		property DateTime timeOut {

			DateTime get() 
			{
				return(_timeOut);
			}
		}

		property bool success {

			bool get() 
			{
				return(_success);
			}
		}

	};
}