using AutoMapper;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DataTransferObjects;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Progress;
using Microsoft.Win32;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;

namespace MediaViewer.TagEditor
{
    class TagEditorImportViewModel : CloseableBindableBase, ICancellableOperationProgress
    {
        public TagEditorImportViewModel()
        {
            WindowTitle = "Importing Tags";
            WindowIcon = "pack://application:,,,/Resources/Icons/tag.ico";

            TokenSource = new CancellationTokenSource();

            InfoMessages = new ObservableCollection<String>();

            CancelCommand = new Command(() =>
            {
                TokenSource.Cancel();
            });

            OkCommand = new Command(new Action(() => OnClosingRequest()));
            OkCommand.IsExecutable = false;
        }
     
        public void import(String filename)
        {
           
            XmlTextReader inFile = null;
            try
            {
                inFile = new XmlTextReader(filename);
                Type[] knownTypes = new Type[] { typeof(TagDTO), typeof(TagCategoryDTO) };

                DataContractSerializer tagSerializer = new DataContractSerializer(typeof(List<TagDTO>), knownTypes);

                List<Tag> tags = new List<Tag>();
                List<TagDTO> tagDTOs = (List<TagDTO>)tagSerializer.ReadObject(inFile);

                foreach (TagDTO tagDTO in tagDTOs)
                {
                    var tag = Mapper.Map<TagDTO, Tag>(tagDTO, new Tag());
                    tags.Add(tag);
                }

                TotalProgressMax = tags.Count;
                TotalProgress = 0;

                using (TagDbCommands tagCommands = new TagDbCommands())
                {
                    foreach (Tag tag in tags)
                    {
                        if (TokenSource.Token.IsCancellationRequested == true) return;

                        ItemInfo = "Merging: " + tag.Name;
                        ItemProgress = 0;
                        tagCommands.merge(tag);
                        ItemProgress = 100;
                        TotalProgress++;
                        InfoMessages.Add("Merged: " + tag.Name);
                    }
                }

                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    OkCommand.IsExecutable = true;
                    CancelCommand.IsExecutable = false;
                }));
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error importing tags " + e.Message);
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    OkCommand.IsExecutable = true;
                    CancelCommand.IsExecutable = false;
                }));
            }
            finally
            {
                if (inFile != null)
                {
                    inFile.Dispose();
                }
            }
        }

        CancellationTokenSource TokenSource
        {
            get;
            set;
        }

        int totalProgress;

        public int TotalProgress
        {
            get
            {
                return (totalProgress);
            }
            set
            {               
                SetProperty(ref totalProgress, value);        
            }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get
            {
                return (totalProgressMax);
            }
            set
            {
                SetProperty(ref totalProgressMax,value);
              
            }
        }

        int itemProgress;

        public int ItemProgress
        {
            get
            {
                return (itemProgress);
            }
            set
            {
                SetProperty(ref itemProgress,value);              
            }
        }

        int itemProgressMax;

        public int ItemProgressMax
        {
            get
            {
                return (itemProgressMax);
            }
            set
            {
                SetProperty(ref itemProgressMax,value);          
            }
        }

        String itemInfo;

        public String ItemInfo
        {
            get { return itemInfo; }
            set
            {
                SetProperty(ref itemInfo, value);               
            }
        }
      
        public System.Collections.ObjectModel.ObservableCollection<string> InfoMessages
        {
            get;
            set;
        }

        public Command OkCommand
        {
            get;
            set;
        }

        public Command CancelCommand
        {
            get;
            set;
        }

        public System.Threading.CancellationToken CancellationToken
        {
            get;
            set;
        }

        string windowTitle;

        public string WindowTitle
        {
            get
            {
                return (windowTitle);
            }
            set
            {
                SetProperty(ref windowTitle, value);              
            }
        }

        string windowIcon;

        public string WindowIcon
        {
            get
            {
                return (windowIcon);
            }
            set
            {
                SetProperty(ref windowIcon, value);               
            }
        }

    }
}
