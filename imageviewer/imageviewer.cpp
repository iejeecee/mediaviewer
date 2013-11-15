// imageviewer.cpp : main project file.

#include "stdafx.h"
#include "MainForm.h"

using namespace imageviewer;

[STAThreadAttribute]
int main(array<System::String ^> ^args)
{
	// Enabling Windows XP visual effects before any controls are created
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false); 

	MainForm ^mainForm = gcnew MainForm(args);

	// Create the main window and run it
	Application::Idle += gcnew EventHandler(mainForm, &MainForm::application_Idle);
	Application::Run(mainForm);

	return 0;
}
