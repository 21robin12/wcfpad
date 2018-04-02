namespace WcfPad.UI
{
    public static class CustomCefSettings
    {
        #if DEBUG
            public static bool ShowDevTools = true;
        #else
            public static bool ShowDevTools = false;
        #endif
    }
}
