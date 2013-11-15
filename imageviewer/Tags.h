#pragma once

#include "Database.h"

using namespace System::Data::SqlServerCe;
using namespace System::Data;


namespace imageviewer {

	public ref class Tags
	{

	public:

		static List<String ^> ^getAll() {

			List<String ^> ^tags = gcnew List<String ^>();

			DataSet ^s = Database::query("SELECT name,id FROM Tags");

			for each(DataTable ^result in s->Tables) {

				for each(DataRow ^row in result->Rows) {

					tags->Add((String ^)row->ItemArray[0]);
				}
			}

			return(tags);
		}
	};

}