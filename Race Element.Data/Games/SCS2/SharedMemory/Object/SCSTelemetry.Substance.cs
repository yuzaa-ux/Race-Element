namespace SCSSdkClient.Object;
public sealed partial class SCSTelemetry
{
    /// <summary>
    ///     Contains names to substances in other fields
    /// </summary>
    public sealed class Substance
    {
        /// <summary>
        ///     Index of the substance in-game
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        ///     Name of the substance
        /// </summary>
        public string Value { get; internal set; }
    }
}