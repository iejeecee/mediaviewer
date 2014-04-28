using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DataTransferObjects
{
    class AutoMapperSetup
    {
        public static void Run()
        {
            Mapper.CreateMap<Tag, TagDTO>();
            Mapper.CreateMap<TagDTO, Tag>();
            Mapper.CreateMap<TagCategory, TagCategoryDTO>();
            Mapper.CreateMap<TagCategoryDTO, TagCategory>();
        }
    }
}
