using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShadowSession.Data
{
    [Table("recordings")]
    public class Recording
    {
        [Key]
        [Column("recording_id", Order = 0)]
        public int RecordingId { get; set; }

        [Column("session_id", Order = 1)]
        [ForeignKey("Session")]
        public int SessionId { get; set; }

        [Column("file_path", Order = 2)]
        public string FilePath { get; set; } = default!;

        public virtual Session Session { get; set; } = default!;
    }
}
