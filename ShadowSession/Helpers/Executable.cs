namespace ShadowSession.Helpers
{
    /// <summary>
    /// An executable's essential information
    /// </summary>
    public record Executable
    {
        /// <summary>
        /// Gets or sets the display name of the executable
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Gets or sets the absolute path leading to the executable
        /// </summary>
        public string Path { get; set; } = default!;
    }
}
