using Humanizer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ShadowSession.Data
{

    // TODO: Remove Priority

    [Table("programs")]
    public class Program
    {
        [Key]
        [Column("program_id", Order = 0),]
        public int ProgramId { get; set; }

        [Column("display_name", Order = 1)]
        public string DisplayName { get; set; } = default!;

        [Column("filename", Order = 2)]
        public string Filename { get; set; } = default!;

        [Column("path", Order = 3)]
        public string Path { get; set; } = default!;

        [Column("automatic_recording_enabled", Order = 4)]
        public bool AutomaticRecordingEnabled { get; set; } = false;

        [Column("recording_framerate", Order = 5)]
        public int? RecordingFramerate { get; set; }

        [Column("recording_bitrate", Order = 6)]
        public int? RecordingBitrate { get; set; }

        [Column("is_active", Order = 7)]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Session> Sessions { get; set; } = new HashSet<Session>();

        public virtual bool HasSessions => Sessions.Count != 0;

        public virtual int TotalSessionCount => Sessions.Count;

        public virtual DateTime? FirstSessionDate
        {
            get
            {
                return Sessions
                    .OrderBy(x => x.StartDate)
                    .Select(x => x.StartDate).FirstOrDefault();
            }
        }

        public virtual DateTime? LastSessionDate
        {
            get
            {
                return Sessions
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => x.StartDate)
                    .FirstOrDefault();
            }
        }

        public virtual TimeSpan TotalSessionDuration
        {
            get
            {
                if (Sessions.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                var durations = Sessions
                    .Where(x => x.EndDate.HasValue)
                    .Select(x => x.EndDate!.Value - x.StartDate);

                if (durations.Any())
                {
                    return TimeSpan.FromSeconds(durations.Sum(x => x.TotalSeconds));
                }

                return TimeSpan.Zero;
            }
        }

        public virtual TimeSpan AverageSessionDuration
        {
            get
            {
                if (Sessions.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                var durations = Sessions
                    .Where(x => x.EndDate.HasValue)
                    .Select(x => x.EndDate!.Value - x.StartDate);

                if (durations.Any())
                {
                    return TimeSpan.FromSeconds(durations.Average(x => x.TotalSeconds));
                }

                return TimeSpan.Zero;
            }
        }

        public virtual TimeSpan LastTwoWeeksSessionDuration
        {
            get
            {
                if (Sessions.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                var twoWeeksAgo = DateTime.Now.AddDays(-14);

                var durations = Sessions
                            .Where(x => x.EndDate.HasValue)
                            .Where(x => x.StartDate >= twoWeeksAgo)
                            .Select(x => x.EndDate!.Value - x.StartDate);

                if (durations.Any())
                {
                    return TimeSpan.FromSeconds(durations.Sum(x => x.TotalSeconds));
                }

                return TimeSpan.Zero;
            }
        }
    }
}
