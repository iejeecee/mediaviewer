using AutoMapper;
using MediaViewer.MediaDatabase.DataTransferObjects;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Mvvm;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MediaViewer.MediaDatabase.DbSettings
{
    class TagOperationsViewModel : CancellableOperationProgressBase
    {
        public TagOperationsViewModel()
        {           
            WindowIcon = "pack://application:,,,/Resources/Icons/tag.ico";
                               
            OkCommand.IsExecutable = false;

            TotalProgress = 0;
            TotalProgressMax = 1;
            ItemProgress = 0;
            ItemProgressMax = 100;
        }
     
        public void import(String filename)
        {
            WindowTitle = "Importing Tags";

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
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error importing tags " + e.Message);         
            }
            finally
            {
                operationFinished();

                if (inFile != null)
                {
                    inFile.Dispose();
                }
            }
        }

        public void export(String fileName)
        {
            WindowTitle = "Exporting Tags";

            XmlTextWriter outFile = null;
            try
            {
                if (CancellationToken.IsCancellationRequested) return;

                outFile = new XmlTextWriter(fileName, Encoding.UTF8);
                outFile.Formatting = Formatting.Indented;
                Type[] knownTypes = new Type[] { typeof(TagDTO), typeof(TagCategoryDTO) };

                DataContractSerializer tagSerializer = new DataContractSerializer(typeof(List<TagDTO>), knownTypes);

                using (var tagCommands = new TagDbCommands())
                {
                    tagCommands.Db.Configuration.ProxyCreationEnabled = false;
                    List<Tag> tags = tagCommands.getAllTags(true);
                    TotalProgressMax = tags.Count;
                    TotalProgress = 0;
                    List<TagDTO> tagsDTO = new List<TagDTO>();

                    foreach (Tag tag in tags)
                    {
                        if (CancellationToken.IsCancellationRequested) return;

                        var tagDTO = Mapper.Map<Tag, TagDTO>(tag, new TagDTO());

                        ItemInfo = "Exporting: " + tagDTO.Name;
                        ItemProgress = 0;

                        tagsDTO.Add(tagDTO);

                        ItemProgress = 100;
                        InfoMessages.Add("Exported: " + tagDTO.Name);
                        TotalProgress++;
                    }

                    tagSerializer.WriteObject(outFile, tagsDTO);
                }               
            }
            catch (Exception e)
            {
                InfoMessages.Add("Error exporting tags: " + e.Message);                           
            }
            finally
            {
                operationFinished();    

                if (outFile != null)
                {
                    outFile.Dispose();
                }
            }
        }

        public void clear()
        {
            List<Tag> tags;

            try
            {
                if (CancellationToken.IsCancellationRequested) return;

                using (TagDbCommands tagCommands = new TagDbCommands())
                {
                    tags = tagCommands.getAllUnusedTags();
                    TotalProgress = tags.Count();

                    foreach (Tag tag in tags)
                    {
                        if (CancellationToken.IsCancellationRequested) return;

                        ItemInfo = "Deleting: " + tag.Name;
                        ItemProgress = 0;

                        tagCommands.delete(tag);

                        ItemProgress = 100;
                        InfoMessages.Add("Deleting: " + tag.Name);
                        TotalProgress++;                        
                    }
                }

            }
            catch (Exception e)
            {
                InfoMessages.Add("Error deleting tags: " + e.Message);  
            }
            finally
            {
                operationFinished();   
            }
          
        }

        void operationFinished()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                OkCommand.IsExecutable = true;
                CancelCommand.IsExecutable = false;
            }));
        }

    }
}
