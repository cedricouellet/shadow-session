using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ShadowSession.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Will override any given provider with SQLite, as the application only supports SQLite</remarks>
    public class ShadowSessionContext : DbContext
    {
        public DbSet<Program> Programs { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Recording> Recordings { get; set; }

        public DbSet<UserSetting> UserSettings { get; set; }

        public string DatabasePath { get; }

        public ShadowSessionContext() : this(new DbContextOptions<ShadowSessionContext>())
        {

        }

        public ShadowSessionContext(DbContextOptions<ShadowSessionContext> options) : base(options)
        {
            var databaseDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cedricao",
                "ShadowSession");

            if (!Directory.Exists(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
            }

            DatabasePath = Path.Join(databaseDirectory, "ShadowSession.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabasePath}");

            base.OnConfiguring(optionsBuilder);
        }

        public bool TryAttach<T>(T entity) where T : class
        {
            try
            {
                if (!Set<T>().Local.Contains(entity))
                {
                    Attach(entity);
                    return true;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Any<T>(Func<T, bool> selector) where T: class
        {
            return Set<T>().Any(selector);
        }

        public EntityEntry<T> UpdateBy<T>(T entity, Func<T, bool>? existingEntitySelector = null, IEnumerable<string>? existingPropertyNamesToUpdate = null) where T : class
        {
            existingEntitySelector ??= (x) => x == entity;

            var existingValue = Set<T>().SingleOrDefault(existingEntitySelector);

            if (existingValue != null)
            {
                var entry = Set<T>().Entry(existingValue);

                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.IsPrimaryKey() || property.Metadata.IsShadowProperty())
                    {
                        continue;
                    }

                    var propertyName = property.Metadata.Name;

                    if (existingPropertyNamesToUpdate != null && !existingPropertyNamesToUpdate.Contains(propertyName))
                    {
                        continue;
                    }

                    var newValue = typeof(T).GetProperty(propertyName)?.GetValue(entity);

                    property.CurrentValue = newValue;
                }

                return entry;
            }

            return Set<T>().Add(entity);

        }
    }
}
