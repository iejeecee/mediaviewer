#pragma once
#include "PicasaOAuth2.h"
#include "LoginDialog.h"


namespace imageviewer {


	public ref class PicasaLoginDialog : public LoginDialog
	{
	public:
		PicasaLoginDialog(void)
		{
			authInstance = gcnew PicasaOAuth2();
			Text = L"Picasa Login";
		}
	};
}
