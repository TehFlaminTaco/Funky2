using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using OpenGL;
using OpenGL.CoreUI;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;

namespace Funky.Libs{
    public static class LibDraw{
        public static Dictionary<IntPtr, NativeWindow> contextToWindow = new Dictionary<IntPtr, NativeWindow>();
        public static Dictionary<VarList, uint> textureLists = new Dictionary<VarList, uint>();
        public static Dictionary<VarList, Bitmap> bitmapLists = new Dictionary<VarList, Bitmap>();
        public static Dictionary<VarList, Font> fontLists = new Dictionary<VarList, Font>();

        public static VarList foregroundColor = new VarList();

        private static uint lastWidth = 0;
        private static uint lastHeight = 0;

        private static int CurrentMouse = 0;

        private static Font currentFont = new Font(new FontFamily("Arial"), 12, FontStyle.Regular, GraphicsUnit.Pixel);
        private static System.Drawing.Text.TextRenderingHint FontAA = System.Drawing.Text.TextRenderingHint.AntiAlias;
        private static uint fontTexture;

        public static VarList Generate(){
            VarList draw = new VarList();

            foregroundColor[0] = 0.0f;
            foregroundColor[1] = 0.0f;
            foregroundColor[2] = 0.0f;
            foregroundColor[3] = 1.0f;

            draw["newWindow"] = new VarFunction(dat => {
                VarList windowList = new VarList();
                windowList["onDraw"] = new VarEvent("onDraw");
                windowList["onKeyDown"] = new VarEvent("onKeyDown");
                windowList["onKeyUp"] = new VarEvent("onKeyUp");
                windowList["onLoad"] = new VarEvent("onLoad");
                windowList["onMouseDown"] = new VarEvent("onMouseDown");
                windowList["onMouseUp"] = new VarEvent("onMouseUp");
                windowList["onMouseWheel"] = new VarEvent("onMouseWheel");
                Thread t = null;
                t = new Thread(()=>{
                    using(NativeWindowWinNTCustom nw = new NativeWindowWinNTCustom()){
                        nw.ContextCreated += (object s, NativeWindowEventArgs e)=>{windowList.Get("onLoad").Call(new CallData(windowList));};
                        
                        string title = FunkyHelpers.ReadArgument(dat, 0, "title", "Funky2").asString();
                        uint width   = (uint)FunkyHelpers.ReadArgument(dat, 1, "width", 640).asNumber();
                        uint height  = (uint)FunkyHelpers.ReadArgument(dat, 2, "height", 480).asNumber();

                        lastWidth = width;
                        lastHeight = height;

                        try{
                            nw.Create(0,0,width,height,NativeWindowStyle.Overlapped);
                            FunkyHelpers.SetWindowTextW(nw.Handle, title);
                            FunkyHelpers.SetWindowLongW(nw.Handle, FunkyHelpers.GWL_STYLE, FunkyHelpers.GetWindowLong(nw.Handle, FunkyHelpers.GWL_STYLE)|FunkyHelpers.WS_SYSMENU);
                        }catch(System.ComponentModel.Win32Exception){
                            return;
                        }

                        Gl.Enable(EnableCap.Blend);
                        Gl.Enable(EnableCap.Texture2d);
                        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        fontTexture = Gl.GenTexture();

                        nw.Render += DrawWindow(nw, windowList);
                        nw.KeyDown += OnKeyDown(nw, windowList);
                        nw.KeyUp += OnKeyUp(nw, windowList);
                        nw.MouseDown += OnMouseDown(nw, windowList);
                        nw.MouseUp += OnMouseUp(nw, windowList);
                        nw.WorkingMouseWheel += OnMouseWheel(nw, windowList);

                        windowList.meta = WindowMeta(nw, windowList);

                        nw.ContextDestroying += (object s, NativeWindowEventArgs e)=>{
                            FunkyHelpers.EndTask(nw.Handle, false, true);
                        };
                        nw.Show();
                        nw.Run();
                        
                    }
                });
                t.Start();
                return windowList;
            });

            draw["getWidth"] = new VarFunction(dat => lastWidth);
            draw["getHeight"] = new VarFunction(dat => lastHeight);

            draw["loadFont"] = new VarFunction(dat => {
                VarList l = new VarList();
                string family = (string)FunkyHelpers.ReadArgument(dat, 0, "family", "Arial").asString();
                int size      = (int)FunkyHelpers.ReadArgument(dat, 1, "size", 12f).asNumber();
                int AA        = (int)FunkyHelpers.ReadArgument(dat, 2, "AA", 1f).asNumber();
                string style  = FunkyHelpers.ReadArgument(dat, 3, "style", "Regular").asString();
                FontStyle st =  FontStyle.Regular;
                FontStyle.TryParse(style, true, out st);
                Font f = new Font(new FontFamily(family), size, st, GraphicsUnit.Pixel);
                fontLists[l] = f;
                l["AA"] = AA!=0 ? 1 : 0;
                return l;
            });
            draw["setFont"] = new VarFunction(dat => {
                Var font = FunkyHelpers.ReadArgument(dat, 0, "font", Var.nil);
                if(font is VarNull)
                    return draw; // No action
                VarList f = font.asList();
                if(fontLists.ContainsKey(f)){
                    currentFont = fontLists[f];
                    FontAA = f.Get("AA").asBool()?System.Drawing.Text.TextRenderingHint.AntiAlias:System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                }
                return draw;
            });
            draw["loadTexture"] = new VarFunction(dat => {
                uint text = Gl.GenTexture();
                Gl.BindTexture(TextureTarget.Texture2d, text);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
                Bitmap map = new Bitmap(dat.num_args[0].asString());
                BitmapData bm = null;
                try{
                    bm = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadOnly, map.PixelFormat);
                    Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, map.Width, map.Height, 0, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bm.Scan0);
                }finally{
                    if(bm != null)
                        map.UnlockBits(bm);
                }
                VarList l = new VarList();
                textureLists[l] = text;
                bitmapLists[l] = map;
                l["getWidth"] = new VarFunction(dat => map.Width);
                l["getHeight"] = new VarFunction(dat => map.Height);
                //Gl.GenerateMipmap(TextureTarget.Texture2d);
                return l;
            });
            draw["texturedRect"] = draw["image"] = new VarFunction(dat => {
                VarList l = dat.num_args[0].asList();
                if(textureLists.ContainsKey(l)){
                    uint text = textureLists[l];
                    Bitmap map = bitmapLists[l];
                    double x  = (double)FunkyHelpers.ReadArgument(dat, 1, "x", 0.0d).asNumber();
                    double y  = (double)FunkyHelpers.ReadArgument(dat, 2, "y", 0.0d).asNumber();
                    double r  = (double)FunkyHelpers.ReadArgument(dat, 3, "r", 0.0d).asNumber();
                    double sx = (double)FunkyHelpers.ReadArgument(dat, 4, "sx", 1.0d).asNumber();
                    double sy = (double)FunkyHelpers.ReadArgument(dat, 5, "sy", 1.0d).asNumber();
                    double ox = (double)FunkyHelpers.ReadArgument(dat, 6, "ox", 0.0d).asNumber();
                    double oy = (double)FunkyHelpers.ReadArgument(dat, 7, "oy", 0.0d).asNumber();
                    double kx = (double)FunkyHelpers.ReadArgument(dat, 8, "kx", 1.0d).asNumber();
                    double ky = (double)FunkyHelpers.ReadArgument(dat, 9, "ky", 1.0d).asNumber();

                    Gl.PushMatrix();
                        Gl.Translate(x, y, 0d);
                        Gl.Scale(sx, sy, 0d);
                        Gl.Rotate(r, 0.0d, 0.0d, 1.0d);
                        Gl.Translate(-ox*map.Width, -oy*map.Height, 0d);
                        Gl.BindTexture(TextureTarget.Texture2d, text);
                        Gl.Begin(PrimitiveType.Quads);
                            Gl.TexCoord2(0f, 0f);
                            Gl.Vertex2(0d, 0d);
                            Gl.TexCoord2((float)kx, 0f);
                            Gl.Vertex2(map.Width, 0d);
                            Gl.TexCoord2((float)kx, (float)ky);
                            Gl.Vertex2(map.Width, map.Height);
                            Gl.TexCoord2(0f, (float)ky);
                            Gl.Vertex2(0d, map.Height);
                        Gl.End();
                    Gl.PopMatrix();
                    Gl.BindTexture(TextureTarget.Texture2d, 0);
                }
                return draw;
            });
            draw["push"] = new VarFunction(dat => {Gl.PushMatrix(); return draw;});
            draw["pop"] = new VarFunction(dat => {Gl.PopMatrix(); return draw;});
            draw["translate"] = new VarFunction(dat => {
                double x = (double)FunkyHelpers.ReadArgument(dat, 0, "x", 0.0d).asNumber();
                double y = (double)FunkyHelpers.ReadArgument(dat, 1, "y", 0.0d).asNumber();
                Gl.Translate(x, y, 0.0d);
                return draw;
            });
            draw["rotate"] = new VarFunction(dat => {
                double r = (double)FunkyHelpers.ReadArgument(dat, 0, "r", 0.0d).asNumber();
                Gl.Rotate(r, 0.0d, 0.0d, 1.0d);
                return draw;
            });
            draw["scale"] = new VarFunction(dat => {
                double x = (double)FunkyHelpers.ReadArgument(dat, 0, "x", 0.0d).asNumber();
                double y = (double)FunkyHelpers.ReadArgument(dat, 1, "y", 0.0d).asNumber();
                Gl.Scale(x, y, 0.0d);
                return draw;
            });
            draw["rect"] = draw["rectangle"] = draw["box"] = new VarFunction(dat => {
                double x = (double)FunkyHelpers.ReadArgument(dat, 0, "x", 0.0d).asNumber();
                double y = (double)FunkyHelpers.ReadArgument(dat, 1, "y", 0.0d).asNumber();
                double w = (double)FunkyHelpers.ReadArgument(dat, 2, "w", 0.0d).asNumber();
                double h = (double)FunkyHelpers.ReadArgument(dat, 3, "h", 0.0d).asNumber();

                Gl.Begin(PrimitiveType.Quads);
                    Gl.Vertex2(x, y);
                    Gl.Vertex2(x+w, y);
                    Gl.Vertex2(x+w, y+h);
                    Gl.Vertex2(x, y+h);
                Gl.End();

                return draw;
            });
            draw["poly"] = draw["polygon"] = new VarFunction(dat => {
                // Ensure 6 arguments.
                for(int i=0; i < 6; i++)if(!dat.num_args.ContainsKey(i))return Var.nil;

                Gl.Begin(PrimitiveType.Polygon);
                    for(int i=0; dat.num_args.ContainsKey(i) && dat.num_args.ContainsKey(i+1); i+=2){
                        Gl.Vertex2(dat.num_args[i].asNumber(), dat.num_args[i+1].asNumber());
                    }
                Gl.End();

                return draw;
            });

            draw["text"] = draw["print"] = new VarFunction(dat => {
                string showText = (string)FunkyHelpers.ReadArgument(dat, 0, "text", "").asString();
                float x         = (float)FunkyHelpers.ReadArgument(dat, 1, "x", 0f).asNumber();
                float y         = (float)FunkyHelpers.ReadArgument(dat, 2, "y", 0f).asNumber();
                int ha          = (int)FunkyHelpers.ReadArgument(dat, 3, "ha", -1f).asNumber();
                int va          = (int)FunkyHelpers.ReadArgument(dat, 4, "va", -1f).asNumber();
                float ox        = (float)FunkyHelpers.ReadArgument(dat, 5, "ox", 0f).asNumber();
                float oy        = (float)FunkyHelpers.ReadArgument(dat, 6, "oy", 0f).asNumber();

                ha = (int)Math.Clamp(ha, -1, 1);
                va = (int)Math.Clamp(va, -1, 1);
                StringFormat alignSettings = new StringFormat();
                if(ha < 0)
                    alignSettings.Alignment = StringAlignment.Near;
                else if(ha == 0)
                    alignSettings.Alignment = StringAlignment.Center;
                else
                    alignSettings.Alignment = StringAlignment.Far;
                if(va < 0)
                    alignSettings.LineAlignment = StringAlignment.Near;
                else if(va == 0)
                    alignSettings.LineAlignment = StringAlignment.Center;
                else
                    alignSettings.LineAlignment = StringAlignment.Far;

                Bitmap map;
                SizeF textSize = Graphics.FromImage(new Bitmap(1,1)).MeasureString(showText, currentFont);
                map = new Bitmap((int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));
                using (Graphics g = Graphics.FromImage(map)){
                    g.Clear(Color.Transparent);
                    using (SolidBrush solidBrush = new SolidBrush(Color.White)){
                        g.TextRenderingHint = FontAA;
                        PointF drawPos = new PointF((ha+1)*map.Width/2, (va+1)*map.Height/2);
                        g.DrawString(showText, currentFont, solidBrush, drawPos, alignSettings);
                    }
                }
                uint text =  fontTexture;
                Gl.BindTexture(TextureTarget.Texture2d, text);
                //Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
                //Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
                BitmapData bm = null;
                try{
                    bm = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadOnly, map.PixelFormat);
                    Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, map.Width, map.Height, 0, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bm.Scan0);
                }finally{
                    if(bm != null)
                        map.UnlockBits(bm);
                }
                Gl.GenerateMipmap(TextureTarget.Texture2d);
                Gl.PushMatrix();
                    Gl.Translate(x, y, 0d);
                    Gl.Translate(-ox*map.Width, -oy*map.Height, 0d);
                    Gl.BindTexture(TextureTarget.Texture2d, text);
                    Gl.Begin(PrimitiveType.Quads);
                        Gl.TexCoord2(0f, 0f);
                        Gl.Vertex2(0d, 0d);
                        Gl.TexCoord2(1f, 0f);
                        Gl.Vertex2(map.Width, 0d);
                        Gl.TexCoord2(1f, 1f);
                        Gl.Vertex2(map.Width, map.Height);
                        Gl.TexCoord2(0f, 1f);
                        Gl.Vertex2(0d, map.Height);
                    Gl.End();
                Gl.PopMatrix();
                Gl.BindTexture(TextureTarget.Texture2d, 0);

                return draw;
            });

            draw["setColor"] = new VarFunction(dat => {
                foregroundColor.double_vars[0] = (float)FunkyHelpers.ReadArgument(dat, 0, "r", 0.0f).asNumber();
                foregroundColor.double_vars[1] = (float)FunkyHelpers.ReadArgument(dat, 1, "g", 0.0f).asNumber();
                foregroundColor.double_vars[2] = (float)FunkyHelpers.ReadArgument(dat, 2, "b", 0.0f).asNumber();
                foregroundColor.double_vars[3] = (float)FunkyHelpers.ReadArgument(dat, 3, "a", 1.0f).asNumber();
                Gl.Color4((float)foregroundColor.double_vars[0].asNumber(), (float)foregroundColor.double_vars[1].asNumber(), (float)foregroundColor.double_vars[2].asNumber(), (float)foregroundColor.double_vars[3].asNumber());
                return foregroundColor;
            });

            return draw;
        }
        public static EventHandler<NativeWindowEventArgs> DrawWindow(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowEventArgs e)=>{
                lastWidth = window.Width;
                lastHeight = window.Height;

                Gl.Viewport(0, 0, (int)window.Width, (int)window.Height);
                Gl.Clear(ClearBufferMask.ColorBufferBit);

                Gl.MatrixMode(MatrixMode.Projection);
                Gl.LoadIdentity();
                Gl.Ortho(0.0, window.Width, window.Height, 0.0, 0.0, 1.0);
                Gl.MatrixMode(MatrixMode.Modelview);
                Gl.LoadIdentity();

                l.Get("onDraw").Call(new CallData(l));
            };
        }

        public static EventHandler<NativeWindowKeyEventArgs> OnKeyDown(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowKeyEventArgs e)=>{
                int keyCode = (int)e.Key;
                string keyName = CodeToKey(e.Key);
                CallData cd = new CallData(keyCode, keyName);
                cd.str_args["code"] = keyCode;
                cd.str_args["key"] = keyName;
                l.Get("onKeyDown").Call(cd);
            };
        }
        public static EventHandler<NativeWindowKeyEventArgs> OnKeyUp(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowKeyEventArgs e)=>{
                int keyCode = (int)e.Key;
                string keyName = CodeToKey(e.Key);
                CallData cd = new CallData(keyCode, keyName);
                cd.str_args["code"] = keyCode;
                cd.str_args["key"] = keyName;
                l.Get("onKeyUp").Call(cd);
            };
        }
        public static EventHandler<NativeWindowMouseEventArgs> OnMouseDown(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowMouseEventArgs e)=>{
                int mouseDown = (int)e.Buttons;
                int AddedMice = (mouseDown ^ CurrentMouse)&mouseDown;
                CurrentMouse = mouseDown;
                for(int i=1; i<=255; i<<=1){
                    if((AddedMice&i)>0){
                        string button = Enum.GetName(typeof(MouseButton), i);
                        var x = e.Location.X;
                        var y = e.Location.Y;
                        CallData cd = new CallData(button, x, y);
                        cd.str_args["button"] = button;
                        cd.str_args["x"] = x;
                        cd.str_args["y"] = y;
                        l.Get("onMouseDown").Call(cd);
                    }
                }    
            };
        }
        public static EventHandler<NativeWindowMouseEventArgs> OnMouseUp(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowMouseEventArgs e)=>{
                int mouseDown = (int)e.Buttons;
                int RemovedMice = (mouseDown ^ CurrentMouse)&~mouseDown;
                CurrentMouse = mouseDown;
                for(int i=1; i<=255; i<<=1){
                    if((RemovedMice&i)>0){
                        string button = Enum.GetName(typeof(MouseButton), i);
                        var x = e.Location.X;
                        var y = e.Location.Y;
                        CallData cd = new CallData(button, x, y);
                        cd.str_args["button"] = button;
                        cd.str_args["x"] = x;
                        cd.str_args["y"] = y;
                        l.Get("onMouseUp").Call(cd);
                    }
                }    
            };
        }
        public static EventHandler<NativeWindowMouseEventArgsWorking> OnMouseWheel(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowMouseEventArgsWorking e)=>{
                int delta = e.WheelTicks;
                var x = e.Location.X;
                var y = e.Location.Y;
                CallData cd = new CallData(delta, x, y);
                cd.str_args["delta"] = delta;
                cd.str_args["x"] = x;
                cd.str_args["y"] = y;
                l.Get("onMouseWheel").Call(cd); 
            };
        }

        public static VarList WindowMeta(NativeWindow window, VarList l){
            VarList meta = new VarList();
            VarList metafuncs = new VarList();

            meta["get"] = new VarFunction(d=>metafuncs.Get(d.num_args[1]));

            metafuncs["isKeyDown"] = new VarFunction(d=>{
                if(!d.num_args.ContainsKey(0))
                    return Var.nil;
                Var key = d.num_args[0];

                if(key is VarString)
                    return window.IsKeyPressed(KeyToCode(key.asString()))?1:0;
                else if(key is VarNumber)
                    return window.IsKeyPressed((KeyCode)key.asNumber().value)?1:0;
                return Var.nil;
            });

            metafuncs["setTitle"] = new VarFunction(d=>{
                Var a = FunkyHelpers.ReadArgument(d, 0, "title", Var.nil);
                if(a != Var.nil)
                    FunkyHelpers.SetWindowTextW(window.Handle, a.asString());
                return a;
            });

            return meta;
        }

        public static string CodeToKey(KeyCode code){
            return Enum.GetName(typeof(KeyCode), code);
        }
        public static KeyCode KeyToCode(string name){
            KeyCode outCode = KeyCode.None;
            Enum.TryParse(name, out outCode);
            return outCode;
        }
    }
}