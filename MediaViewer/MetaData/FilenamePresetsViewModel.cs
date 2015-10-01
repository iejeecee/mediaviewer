using MediaViewer.Model.Settings;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using MediaViewer.Model.Utils;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Properties;

namespace MediaViewer.MetaData
{
    class FilenamePresetsViewModel : CloseableBindableBase
    {

        public FilenamePresetsViewModel()
        {
            NewPreset = "";
        
            filenamePresets = Settings.Default.FilenamePresets;

            AddNewPresetCommand = new Command(new Action(() =>
            {
                if (String.IsNullOrEmpty(NewPreset) || String.IsNullOrWhiteSpace(NewPreset)) return;

                filenamePresets.Add(NewPreset);
                NewPreset = "";
            }));

            dateFormats = new ObservableCollection<string>();
            dateFormats.Add("d: 6/15/2008");
            dateFormats.Add("D: Sunday, June 15, 2008");
            dateFormats.Add("f: Sunday, June 15, 2008 9-15 PM");
            dateFormats.Add("F: Sunday, June 15, 2008 9-15-07 PM");
            dateFormats.Add("g: 6/15/2008 9-15 PM");
            dateFormats.Add("G: 6/15/2008 9-15-07 PM");
            dateFormats.Add("m: June 15");
            dateFormats.Add("o: 2008-06-15T21-15-07.0000000");
            dateFormats.Add("R: Sun, 15 Jun 2008 21-15-07 GMT");
            dateFormats.Add("s: 2008-06-15T21-15-07");
            dateFormats.Add("t: 9-15 PM");
            dateFormats.Add("T: 9-15-07 PM");
            dateFormats.Add("u: 2008-06-15 21-15-07Z");
            dateFormats.Add("U: Monday, June 16, 2008 4-15-07 AM");
            dateFormats.Add("y: June, 2008");

            SelectedDateFormat = DateFormats[3];

            counterValue = "0001";

            SelectCommand = new Command(new Action(() =>
            {
                OnClosingRequest(new DialogEventArgs(DialogMode.SUBMIT));
            }));

            SelectCommand.IsExecutable = false;

            DeleteCommand = new Command(new Action(() =>
            {
                FilenamePresets.Remove(SelectedPreset);

            }));

            CancelCommand = new Command(new Action(() =>
                {

                    OnClosingRequest(new DialogEventArgs(DialogMode.CANCEL));
                }));

            DeleteCommand.IsExecutable = false;

            InsertCounterCommand = new Command<int?>(new Action<int?>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex.Value, "\"" + MetaDataUpdateViewModel.counterMarker + CounterValue + "\"");
                }
                catch (Exception e)
                {
                    Logger.Log.Error(e);
                }

            }));

            InsertFilenameCommand = new Command<int?>(new Action<int?>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex.Value, "\"" + MetaDataUpdateViewModel.oldFilenameMarker + "\"");
                }
                catch (Exception e)
                {
                    Logger.Log.Error(e);
                }

            }));

            InsertResolutionCommand = new Command<int?>(new Action<int?>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex.Value, "\"" + MetaDataUpdateViewModel.resolutionMarker + "\"");
                }
                catch (Exception e)
                {
                    Logger.Log.Error(e);
                }

            }));

            InsertDateCommand = new Command<int?>(new Action<int?>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex.Value, "\"" + MetaDataUpdateViewModel.dateMarker
                        + SelectedDateFormat.Substring(0, SelectedDateFormat.IndexOf(':')) + "\"");
                }
                catch (Exception e)
                {
                    Logger.Log.Error(e);
                }

            }));

            InsertReplaceCommand = new Command<int?>(new Action<int?>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex.Value, "\"" + MetaDataUpdateViewModel.replaceMarker + MatchString + ";" + ReplaceString + "\"");
                }
                catch (Exception e)
                {
                    Logger.Log.Error(e);
                }

            }));

            MatchString = "";
            ReplaceString = "";

        }

        ObservableCollection<String> dateFormats;

        public ObservableCollection<String> DateFormats
        {
            get { return dateFormats; }
            set { dateFormats = value; }
        }

        String selectedDateFormat;

        public String SelectedDateFormat
        {
            get { return selectedDateFormat; }
            set
            {               
                SetProperty(ref selectedDateFormat, value);
            }
        }

        ObservableCollection<String> filenamePresets;

        public ObservableCollection<String> FilenamePresets
        {
            get { return filenamePresets; }
            set { filenamePresets = value; }
        }

        String counterValue;

        public String CounterValue
        {
            get { return counterValue; }
            set
            {
                SetProperty(ref counterValue, value);
             
                if (counterValue != null)
                {
                    int temp;
                    bool isInteger = int.TryParse(counterValue, out temp);
                    if (!isInteger)
                    {
                        InsertCounterCommand.IsExecutable = false;
                        throw new ArgumentException("Counter value must be a number");
                    }
                    else
                    {
                        InsertCounterCommand.IsExecutable = true;
                    }
                }
                
            }
        }

        String newPreset;

        public String NewPreset
        {
            get { return newPreset; }
            set
            {               
                SetProperty(ref newPreset, value);
            }
        }

        String selectedPreset;

        public String SelectedPreset
        {
            get { return selectedPreset; }
            set
            {
                SetProperty(ref selectedPreset, value);
               
                DeleteCommand.IsExecutable = SelectCommand.IsExecutable = (selectedPreset == null) ? false : true;

            }
        }

        String matchString;

        public String MatchString
        {
            get { return matchString; }
            set
            {
                SetProperty(ref matchString, value);             

                if (String.IsNullOrEmpty(matchString))
                {
                    InsertReplaceCommand.IsExecutable = false;

                }
                else if (FileUtils.containsIllegalFileNameChars(matchString) || FileUtils.containsIllegalFileNameChars(replaceString))
                {
                    InsertReplaceCommand.IsExecutable = false;
                    throw new ArgumentException("Filename string contains illegal characters");
                }
                else
                {
                    InsertReplaceCommand.IsExecutable = true;
                }
                             
            }
        }

        String replaceString;

        public String ReplaceString
        {
            get { return replaceString; }
            set
            {
                SetProperty(ref replaceString, value);

                if (String.IsNullOrEmpty(matchString))
                {
                    InsertReplaceCommand.IsExecutable = false;
                }
                else if (FileUtils.containsIllegalFileNameChars(matchString) || FileUtils.containsIllegalFileNameChars(replaceString))
                {
                    InsertReplaceCommand.IsExecutable = false;
                    throw new ArgumentException("Filename string contains illegal characters");
                }
                else
                {
                    InsertReplaceCommand.IsExecutable = true;
                }
                                          
            }
        }

        public Command AddNewPresetCommand { get; set; }
        public Command DeleteCommand  { get; set; }       
        public Command SelectCommand  { get; set; }
        public Command CancelCommand  { get; set; }     
        public Command<int?> InsertCounterCommand  { get; set; }
        public Command<int?> InsertDateCommand { get; set; }
        public Command<int?> InsertFilenameCommand { get; set; }
        public Command<int?> InsertResolutionCommand { get; set; }
        public Command<int?> InsertReplaceCommand  { get; set; }
     
    }
}
