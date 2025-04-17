using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShadowSession.Data
{
    [Table("sessions")]
    public class Session
    {
        [Key]
        [Column("session_id", Order = 0)]
        public int SessionId { get; set; }

        [Column("program_id", Order = 1)]
        [ForeignKey("Program")]
        public int ProgramId { get; set; }

        [Column("start_date", Order = 2)]
        public DateTime StartDate { get; set; }

        [Column("end_date", Order = 3)]
        public DateTime? EndDate { get; set; }

        public virtual ICollection<Recording> Recordings { get; set; } = new HashSet<Recording>();

        public TimeSpan Duration
        {
            get
            {
                if (!EndDate.HasValue)
                {
                    return TimeSpan.Zero;
                }

                return EndDate.Value - StartDate;
            }
        }

        public virtual Program Program { get; set; } = default!;
    }
}
