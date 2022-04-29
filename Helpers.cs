using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq.Expressions;
namespace Funky{
    public static class FunkyHelpers{

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

        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr hCursor);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("kernel32", SetLastError=true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }
}

static class DelegateCreator {
    public static readonly Func<Type[], Type> MakeNewCustomDelegate = (Func<Type[], Type>) Delegate.CreateDelegate(
      typeof(Func<Type[], Type>),
      typeof(Expression).Assembly.GetType("System.Linq.Expressions.Compiler.DelegateHelpers").GetMethod(
        "MakeNewCustomDelegate",
        BindingFlags.NonPublic | BindingFlags.Static
      )
    );
    static HashSet<(Type ret, Type[] parms, Type t)> knownTypes = new HashSet<(Type ret, Type[] parms, Type t)>();
    public static Type NewDelegateType(Type ret, params Type[] parameters) {
      if(knownTypes.Any(e=>e.ret==ret && e.parms.SequenceEqual(parameters))){
        return knownTypes.First(e=>e.ret==ret && e.parms.SequenceEqual(parameters)).t;
      }
      var offset = parameters.Length;
      var oldParams = parameters.ToList().ToArray();
      Array.Resize(ref parameters, offset + 1);
      parameters[offset] = ret;
      var delg = MakeNewCustomDelegate(parameters);
      knownTypes.Add((ret, oldParams, delg));
      return delg;
    }
  }