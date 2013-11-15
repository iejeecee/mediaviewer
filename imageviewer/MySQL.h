#pragma once

using namespace System;
using namespace System::Data;

using namespace MySql::Data;
using namespace MySql::Data::MySqlClient;

namespace imageviewer {

public ref class MySQL
{
	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

private:

	static  MySqlConnection ^conn;

public:

	static MySQL() 
    {
        String ^connStr = "server=localhost;user=root;database=web;port=3306;password=2bGreat?;";
        conn = gcnew MySqlConnection(connStr);
        try
        {
            
            conn->Open();
         
        }
        catch(Exception ^e)
        {
			log->Error(e->Message);
        }

    }

	static DataSet ^query(String ^sql)
	{
		DataSet ^ds = nullptr;

		try
		{
			
			//SqlCeCommand ^command = gcnew SqlCeCommand(sql);
			MySqlDataAdapter ^dataAdapter = gcnew MySqlDataAdapter(sql, conn);
			ds = gcnew DataSet();
			dataAdapter->Fill(ds);
		}
		catch (Exception ^e) {

			log->Error(e->Message);
		}

		return ds;
	}
};

}
