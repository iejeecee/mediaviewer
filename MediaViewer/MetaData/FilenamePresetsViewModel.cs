using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData
{
    class FilenamePresetsViewModel : CloseableObservableObject
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FilenamePresetsViewModel()
        {
            NewPreset = "";

            filenamePresets = new ObservableCollection<string>();
            foreach (String preset in MediaViewer.Settings.AppSettings.Instance.FilenamePresets)
            {
                filenamePresets.Add(preset);
            }

            addNewPresetCommand = new Command(new Action(() =>
            {
                if (String.IsNullOrEmpty(NewPreset) || String.IsNullOrWhiteSpace(NewPreset)) return;

                filenamePresets.Add(NewPreset);
                MediaViewer.Settings.AppSettings.Instance.FilenamePresets.Add(NewPreset);
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

            counterValue = 0;

            SelectCommand = new Command(new Action(() =>
            {             
                OnClosingRequest(new DialogEventArgs(DialogMode.SUBMIT));
            }));

            SelectCommand.CanExecute = false;

            DeleteCommand = new Command(new Action(() =>
            {
                FilenamePresets.Remove(SelectedPreset);
                MediaViewer.Settings.AppSettings.Instance.FilenamePresets.Remove(SelectedPreset);
            }));

            cancelCommand = new Command(new Action(() =>
                {
                   
                    OnClosingRequest(new DialogEventArgs(DialogMode.CANCEL));
                }));

            DeleteCommand.CanExecute = false;

            insertCounterCommand = new Command<int>(new Action<int>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex, "\"" + MetaDataUpdateViewModel.counterMarker + CounterValue.ToString() + "\"");
                }
                catch (Exception e)
                {
                    log.Error(e);
                }

            }));

            insertFilenameCommand = new Command<int>(new Action<int>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex, "\"" + MetaDataUpdateViewModel.oldFilenameMarker + "\"");
                }
                catch (Exception e)
                {
                    log.Error(e);
                }

            }));

            insertResolutionCommand = new Command<int>(new Action<int>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex, "\"" + MetaDataUpdateViewModel.resolutionMarker + "\"");
                }
                catch (Exception e)
                {
                    log.Error(e);
                }

            }));

            insertDateCommand = new Command<int>(new Action<int>((startIndex) =>
            {
                try
                {
                    NewPreset = NewPreset.Insert(startIndex, "\"" + MetaDataUpdateViewModel.dateMarker
                        + SelectedDateFormat.Substring(0, SelectedDateFormat.IndexOf(':')) + "\"");
                }
                catch (Exception e)
                {
                    log.Error(e);
                }

            }));


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
                selectedDateFormat = value;
                NotifyPropertyChanged();
            }
        }

        ObservableCollection<String> filenamePresets;

        public ObservableCollection<String> FilenamePresets
        {
            get { return filenamePresets; }
            set { filenamePresets = value; }
        }

        int counterValue;

        public int CounterValue
        {
            get { return counterValue; }
            set
            {
                counterValue = value;
                NotifyPropertyChanged();
            }
        }

        String newPreset;

        public String NewPreset
        {
            get { return newPreset; }
            set
            {
                newPreset = value;
                NotifyPropertyChanged();
            }
        }

        String selectedPreset;

        public String SelectedPreset
        {
            get { return selectedPreset; }
            set
            {
                selectedPreset = value;

                deleteCommand.CanExecute = selectCommand.CanExecute = (selectedPreset == null) ? false : true;

                NotifyPropertyChanged();
            }
        }

        Command addNewPresetCommand;

        public Command AddNewPresetCommand
        {
            get { return addNewPresetCommand; }
            set { addNewPresetCommand = value; }
        }

        Command deleteCommand;

        public Command DeleteCommand
        {
            get { return deleteCommand; }
            set { deleteCommand = value; }
        }

        Command selectCommand;

        public Command SelectCommand
        {
            get { return selectCommand; }
            set { selectCommand = value; }
        }

        Command cancelCommand;

        public Command CancelCommand
        {
            get { return cancelCommand; }
            set { cancelCommand = value; }
        }

        Command<int> insertCounterCommand;

        public Command<int> InsertCounterCommand
        {
            get { return insertCounterCommand; }
            set { insertCounterCommand = value; }
        }

        Command<int> insertDateCommand;

        public Command<int> InsertDateCommand
        {
            get { return insertDateCommand; }
            set { insertDateCommand = value; }
        }

        Command<int> insertFilenameCommand;

        public Command<int> InsertFilenameCommand
        {
            get { return insertFilenameCommand; }
            set { insertFilenameCommand = value; }
        }

        Command<int> insertResolutionCommand;

        public Command<int> InsertResolutionCommand
        {
            get { return insertResolutionCommand; }
            set { insertResolutionCommand = value; }
        }
    }
}
