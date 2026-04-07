namespace LSDE.Demo
{
    /// <summary>
    /// Single-select enum for choosing a locale in the editor Inspector.
    /// Used by <see cref="WebGlSceneController"/> for the editor test dropdown.
    /// Maps to LSDE locale codes: "fr", "en", "ja", "zh".
    /// </summary>
    public enum DemoLocale
    {
        French = 0,
        English = 1,
        Japanese = 2,
        Chinese = 3,
    }
}
