internal static class CommandLine {

    public static bool NoSplash;
    public static bool Editor;

    public static void Init() {

        foreach (var arg in Environment.GetCommandLineArgs()) {

            if (arg.Equals("nosplash", StringComparison.InvariantCultureIgnoreCase)) NoSplash = true;
            if (arg.Equals("editor",   StringComparison.InvariantCultureIgnoreCase)) Editor   = true;
        }
    }
}