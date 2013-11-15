#pragma once

#include "HttpRequest.h"
#include "AuthInfo.h"

using namespace System;
using namespace System::Threading;
using namespace System::Collections::Generic;

using namespace Newtonsoft::Json;

namespace imageviewer {


	public ref class OAuth2
	{

	protected:

		String ^_authCode;
		String ^_clientId;
		String ^_clientSecret;
		String ^_redirectUri;

		String ^url;
		int timeOutSeconds;

		OAuth2() {

			url = "https://accounts.google.com/o/oauth2/token";
			timeOutSeconds = 60;
		}

	public:

		property String ^authCode {

			String ^get() 
			{
				return(_authCode);
			}
		}

		property String ^clientId {

			String ^get() 
			{
				return(_clientId);
			}
		}

		property String ^clientSecret {

			String ^get() {

				return(_clientSecret);
			}
		}

		property String ^redirectUri {

			 String ^get() {

				 return(_redirectUri);
			 }
		}

		AuthInfo ^authenticate(String ^authCode) {

			_authCode = authCode;

			String ^body = "code=" + authCode + "&redirect_uri=" + redirectUri +
				"&client_id=" + clientId + "&client_secret=" + clientSecret + "&grant_type=authorization_code";

			String ^responseText;

			HttpWebResponse ^response = HttpRequest::postRequest(url, body, timeOutSeconds);

			responseText = HttpRequest::responseToString(response, timeOutSeconds);

			Dictionary<String ^, Object ^> ^values = JsonConvert::DeserializeObject<Dictionary<String ^, Object ^> ^>(responseText);

			String ^accessToken = values["access_token"]->ToString();
			String ^refreshToken = values["refresh_token"]->ToString();
			int tokenTimeout = Convert::ToInt32(values["expires_in"]->ToString());

			AuthInfo ^info = gcnew AuthInfo(accessToken, refreshToken, tokenTimeout);

			return(info);
		}

		AuthInfo ^refresh(AuthInfo ^info) {

			String ^body = "client_id=" + clientId + "&client_secret=" + clientSecret + "&refresh_token=" + info->refreshToken + "&grant_type=refresh_token";

			String ^responseText;

			HttpWebResponse ^response = HttpRequest::postRequest(url, body, timeOutSeconds);

			responseText = HttpRequest::responseToString(response, timeOutSeconds);

			Dictionary<String ^, Object ^> ^values = JsonConvert::DeserializeObject<Dictionary<String ^, Object ^> ^>(responseText);

			String ^accessToken = values["access_token"]->ToString();
			String ^refreshToken = values["refresh_token"]->ToString();
			int tokenTimeout = Convert::ToInt32(values["expires_in"]->ToString());

			AuthInfo ^refreshedInfo = gcnew AuthInfo(accessToken, refreshToken, tokenTimeout);

			return(refreshedInfo);
		}

	};
}