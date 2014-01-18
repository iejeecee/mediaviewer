using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class PresetMetadataDbCommands : DbCommands
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PresetMetadataDbCommands(MediaDatabaseContext existingContext = null) : base(existingContext) {

        }

        public List<PresetMetadata> getAllPresets()
        {
            List<PresetMetadata> presets = new List<PresetMetadata>();

            foreach (PresetMetadata preset in Db.PresetMetadataSet.OrderBy(x => x.Name))
            {                
                presets.Add(preset);
            }

            return (presets);
        }

        public PresetMetadata getPresetMetadataById(int id) {

            PresetMetadata result = Db.PresetMetadataSet.Include("Tags").FirstOrDefault(x => x.Id == id);

            return (result);
        }

        public PresetMetadata createPresetMetadata(PresetMetadata preset)
        {
            if (String.IsNullOrEmpty(preset.Name) || String.IsNullOrWhiteSpace(preset.Name))
            {
                throw new DbEntityValidationException("Error updating presetMetadata, name cannot be null, empty or whitespace");
            }

            PresetMetadata newPreset = new PresetMetadata();

            Db.PresetMetadataSet.Add(newPreset);
            Db.Entry<PresetMetadata>(newPreset).CurrentValues.SetValues(preset);
            newPreset.Id = 0;
            
            TagDbCommands tagCommands = new TagDbCommands(Db);

            foreach (Tag tag in preset.Tags)
            {
                Tag addTag = tagCommands.getTagById(tag.Id);

                if (addTag != null)
                {
                    newPreset.Tags.Add(addTag);
                }

            }

            Db.SaveChanges();

            return (newPreset);
        }

        public PresetMetadata updatePresetMetadata(PresetMetadata updatePreset)
        {
            if (String.IsNullOrEmpty(updatePreset.Name) || String.IsNullOrWhiteSpace(updatePreset.Name))
            {
                throw new DbEntityValidationException("Error updating presetMetadata, name cannot be null, empty or whitespace");
            }

            PresetMetadata preset = getPresetMetadataById(updatePreset.Id);

            if (preset == null)
            {
                throw new DbEntityValidationException("Cannot update non-existing tag id: " + updatePreset.Id.ToString());
            }
            Db.Entry<PresetMetadata>(preset).CurrentValues.SetValues(updatePreset);
           
            preset.Tags.Clear();

            TagDbCommands tagCommands = new TagDbCommands(Db);

            foreach (Tag updateTag in updatePreset.Tags)
            {
                Tag tag = tagCommands.getTagById(updateTag.Id);

                if (tag == null)
                {
                    log.Warn("Cannot add non-existent tag: " + updateTag.Id.ToString() + " to presetMetadata: " + preset.Id.ToString());
                    continue;
                }

                preset.Tags.Add(tag);
            }

            Db.SaveChanges();

            return (preset);
        }

        public void deletePresetMetadata(PresetMetadata preset)
        {
            PresetMetadata deletePreset = getPresetMetadataById(preset.Id);

            if (deletePreset == null)
            {
                throw new DbEntityValidationException("Cannot delete non-existing presetMetadata: " + preset.Id.ToString());
            }
         
            Db.PresetMetadataSet.Remove(deletePreset);
            Db.SaveChanges();
        }
    }
}
