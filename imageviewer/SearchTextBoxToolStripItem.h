#pragma once
#include "SearchTextBoxControl.h"

using namespace System::Windows::Forms;
using namespace System::Windows::Forms::Design;

namespace imageviewer {

[ToolStripItemDesignerAvailabilityAttribute(ToolStripItemDesignerAvailability::All)]
public ref class SearchTextBoxToolStripItem : public ToolStripControlHost
{
private:

   property SearchTextBoxControl ^SearchTextBox
   {
      SearchTextBoxControl ^get()
      {
         return static_cast<SearchTextBoxControl ^>(Control);
      }
   }

   void searchTextBoxControl_DoSearch(Object ^sender, MediaSearchState ^e) {

	   DoSearch(this, e);
   }

protected:
   // Subscribe and unsubscribe the control events you wish to expose. 
   virtual void OnSubscribeControlEvents( System::Windows::Forms::Control^ c ) override
   {
      // Call the base so the base events are connected.
      __super::OnSubscribeControlEvents( c );

      // Cast the control to a MonthCalendar control.
      SearchTextBoxControl ^searchTextBox = (SearchTextBoxControl ^)c;

      // Add the event.
	  searchTextBox->DoSearch += 
		  gcnew EventHandler<MediaSearchState ^>(this, &SearchTextBoxToolStripItem::searchTextBoxControl_DoSearch);

   }

   virtual void OnUnsubscribeControlEvents( System::Windows::Forms::Control^ c ) override
   {

      // Call the base method so the basic events are unsubscribed.
      __super::OnUnsubscribeControlEvents( c );

       // Cast the control to a MonthCalendar control.
      SearchTextBoxControl ^searchTextBox = (SearchTextBoxControl ^)c;

      // Add the event.
	  searchTextBox->DoSearch -= 
		  gcnew EventHandler<MediaSearchState ^>(this, &SearchTextBoxToolStripItem::searchTextBoxControl_DoSearch);

   }

public:

   event EventHandler<MediaSearchState ^> ^DoSearch;

   // Call the base constructor passing in a MonthCalendar instance.
   SearchTextBoxToolStripItem() : ToolStripControlHost( gcnew SearchTextBoxControl() ) 
   {
   }
  
   property String ^Query
   {
      // Expose the MonthCalendar.FirstDayOfWeek as a property.
      String ^get()
      {
         return SearchTextBox->Query;
      }

      void set(String ^query)
      {
         SearchTextBox->Query = query;
      }
   }




};

}