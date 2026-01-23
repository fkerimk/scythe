using MoonSharp.Interpreter;

public static class SafeExec {

    public static void Try(Action action) {

        try { action.Invoke(); }
        
        catch (Exception) {
            
            //Console.WriteLine(e);
            //throw;
        }
    }
    
    public static void LuaCall(Action action) {
        
        try { action.Invoke(); } catch (InterpreterException ex) {

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"[LUA] ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"{ex.DecoratedMessage}");
            Console.ResetColor();
        }
        catch (Exception ex) {
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[SCYTHE] ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"{ex.Message}");
            Console.ResetColor();
        }
    }
}