using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSearchPlugin
{
    public class ImageSearchSettings
    {
        public String SafeSearch { get; set; }
        public String Size { get; set; }
        public String Layout { get; set; }
        public String Type { get; set; }
        public String People { get; set; }
        public String Color { get; set; }

        public bool IsAskDownloadPath { get; set; }
        public bool IsCurrentDownloadPath { get; set; }
        public bool IsFixedDownloadPath { get; set; }
        public String FixedDownloadPath { get; set; }
        public ObservableCollection<String> FixedDownloadPathHistory { get; set; }

        public void SetDefaults()
        {
            if (SafeSearch == null)
            {
                SafeSearch = ImageSearchViewModel.safeSearch[0];
                Size = ImageSearchViewModel.size[0];
                Layout = ImageSearchViewModel.layout[0];
                Type = ImageSearchViewModel.type[0];
                People = ImageSearchViewModel.people[0];
                Color = ImageSearchViewModel.color[0];

                IsAskDownloadPath = true;
                IsCurrentDownloadPath = false;
                IsFixedDownloadPath = false;

                FixedDownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                FixedDownloadPathHistory = new ObservableCollection<string>();
            }
        }
    }
}
