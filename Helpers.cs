using System;
using System.Runtime.InteropServices;
namespace Funky{
    public static class FunkyHelpers{
        public static Var ReadArgument(CallData cd, int index, string name, Var def = null, Exception onfail = null){
            if(cd.str_args.ContainsKey(name))
                return cd.str_args[name];
            else if (cd.num_args.ContainsKey(index))
                return cd.num_args[index];
            else if (onfail != null)
                throw onfail;
            else if (def != null)
                return def;
            return Var.nil;
        }

        [DllImport("user32.dll")]
        public static extern bool SetWindowTextW(IntPtr hWnd, string text);
        [DllImport("user32.dll")]
        public static extern bool SetWindowText(IntPtr hWnd, string text);

        public static int GWL_STYLE = -16;
        public static int WS_SYSMENU = 0x80000 | 0x20000 | 0x10000;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLongW(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLongW(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int EndTask(IntPtr hWnd, bool fShutDown, bool force);
    }
}