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
    class TagEditorImportViewModel : CancellableOperationProgressBase
    {
        public TagEditorImportViewModel()
        {
            WindowTitle = "Importing Tags";
            WindowIcon = "pack://application:,,,/Resources/Icons/tag.ico";
        
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
                        if (CancellationToken.IsCancellationRequested == true) return;

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

    }
}
