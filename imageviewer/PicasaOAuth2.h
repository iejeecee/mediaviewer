#pragma once

#include "OAuth2.h"


namespace imageviewer {


	public ref class PicasaOAuth2 : public OAuth2 {

	
	public:

		PicasaOAuth2() {

			_clientId = "873172281457.apps.googleusercontent.com";
			_clientSecret = "SGmQbYwcXFiKk0ItGp6g4ThT";
			_redirectUri = "urn:ietf:wg:oauth:2.0:oob";
		}

		
	};
}