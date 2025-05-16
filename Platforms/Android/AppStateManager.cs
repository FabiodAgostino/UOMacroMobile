namespace UOMacroMobile.Platforms.Android
{
    public static class AppStateManager
    {
        // Stato corrente dell'app (true se in foreground, false se in background)
        public static bool IsAppInForeground { get; set; } = false;
    }
}
