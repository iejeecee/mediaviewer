#pragma once
//http://stackoverflow.com/questions/46827/how-do-you-create-a-foreign-key-relationship-in-a-sql-server-ce-compact-edition

namespace imageviewer {

	using namespace System;
	using namespace System::Data::SqlServerCe;
	using namespace System::Data;

	public ref class Database
	{

	private:

		static SqlCeConnection ^conn;

	public:

/*		
		static Database() 
		{

			try {

				conn = gcnew SqlCeConnection("Data Source=C:\\game\\imageviewer\\database\\MyDatabase#1.sdf; Password = 2bGreat??");
				conn->Open();
				
			} catch (Exception ^e) {

				System::Diagnostics::Debug::Write(e->Message);
			}

		}
*/
		static DataSet ^query(String ^sql)
		{
			DataSet ^ds = nullptr;

			try
			{
				//SqlCeCommand ^command = gcnew SqlCeCommand(sql);
				SqlCeDataAdapter ^dataAdapter = gcnew SqlCeDataAdapter(sql, conn);
				ds = gcnew DataSet();
				dataAdapter->Fill(ds);
			}
			catch (Exception ^e) {

				System::Diagnostics::Debug::Write(e->Message);
			}

			return ds;
		}

		property SqlCeConnection ^Connection {
			
			static SqlCeConnection ^get() {

				return(conn);
			}
			
		}


	};

}