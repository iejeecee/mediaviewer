using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace MediaViewer.MediaDatabase.Mapping
{
    public class AudioMetadataMap : EntityTypeConfiguration<AudioMetadata>
    {
        public AudioMetadataMap()
        {
            // Primary Key
            /*this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.AudioCodec)
                .IsRequired();

            this.Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("AudioMetadata");
            this.Property(t => t.DurationSeconds).HasColumnName("DurationSeconds");
            this.Property(t => t.NrChannels).HasColumnName("NrChannels");
            this.Property(t => t.SamplesPerSecond).HasColumnName("SamplesPerSecond");
            this.Property(t => t.BitsPerSample).HasColumnName("BitsPerSample");
            this.Property(t => t.AudioCodec).HasColumnName("AudioCodec");
            this.Property(t => t.Genre).HasColumnName("Genre");
            this.Property(t => t.Album).HasColumnName("Album");
            this.Property(t => t.TrackNr).HasColumnName("TrackNr");
            this.Property(t => t.TotalTracks).HasColumnName("TotalTracks");
            this.Property(t => t.DiscNr).HasColumnName("DiscNr");
            this.Property(t => t.TotalDiscs).HasColumnName("TotalDiscs");
            this.Property(t => t.AudioContainer).HasColumnName("AudioContainer");
            this.Property(t => t.Id).HasColumnName("Id");*/

            // Relationships
            //this.HasRequired(t => t.BaseMetadata)
            //    .WithOptional(t => t.AudioMetadata);

        }
    }
}
