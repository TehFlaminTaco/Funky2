using System;
using System.Collections.Generic;

namespace Funky
{
    public enum CursorShape{
        Arrow = 32512,
        IBeam = 32513,
        Wait = 32514,
        Cross = 32515,
        UpArrow = 32516,
        Size = 32640,
        Icon = 32641,
        SizeNWSE = 32642,
        SizeNESW = 32643,
        Sizewe = 32644,
        Sizens = 32645,
        SizeAll = 32646,
        No = 32648,
        Hand = 32649,
        AppStarting = 32650,
        Help = 32651
    }
    public static class Cursors {
        private static Dictionary<CursorShape, IntPtr> CursorIcons;
        
        public static IntPtr Get(CursorShape shape){
            if(CursorIcons == null)GenerateCursorIcons();
            return CursorIcons[shape];
        }

        private static void GenerateCursorIcons()
        {
            CursorIcons = new Dictionary<CursorShape, IntPtr>();
            foreach(CursorShape shape in Enum.GetValues(typeof(CursorShape))){
                CursorIcons[shape] = FunkyHelpers.LoadCursor(IntPtr.Zero, (int)shape);
            }
        }
    }
}