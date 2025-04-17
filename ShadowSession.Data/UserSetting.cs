using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShadowSession.Data
{
    [Table("user_settings")]
    public class UserSetting
    {
        [Key]
        [Column("user_setting_id", Order = 0)]
        public int UserSettingId { get; set; }

        [Column("kind", Order = 1)]
        public UserSettingKind Kind { get; set; }

        [Column("key", Order = 2)]
        public string Key { get; set; } = default!;

        [Column("default_value", Order = 3)]
        public string? DefaultValue { get; set; }

        [Column("value", Order = 4)]
        public string? Value { get; set; }

        [Column("display_name", Order = 5)]
        public string DisplayName { get; set; } = default!;

        [Column("description", Order = 6)]
        public string? Description { get; set; }

        [Column("is_value_required", Order = 7)]
        public bool ValueRequired { get; set; } = false;

        [Column("sort_order", Order = 8)]
        public int SortOrder { get; set; } = 0;

        [Column("visible", Order = 9)]
        public bool Visible { get; set; } = false;
    }
}
