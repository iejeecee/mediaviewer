#pragma once

namespace imageviewer {

public ref class PicasaUser
{

	String ^_name;
	String ^_password;

public:

	PicasaUser(String ^name, String ^password) 
	{

		this->name = name;
		this->password = password;
		
	}

	property String ^name {

		String ^get() {

			return(_name);
		}

		void set(String ^name) {

			_name = name;
		}

	}

	property String ^password {

		String ^get() {

			return(_password);
		}

		void set(String ^password) {

			_password = password;
		}

	}
	virtual bool Equals(Object ^obj) override {

		PicasaUser ^user = dynamic_cast<PicasaUser ^>(obj);

		if(user == nullptr) return(false);

		return(user->name->Equals(name));

	}

	virtual String ^ToString() override {

		return(name);
	}

};

}