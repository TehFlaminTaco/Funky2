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
using System.Linq;

namespace Funky.Libs{
    public static class LibDraw{
        #region VarList's real values
        public static Dictionary<IntPtr, NativeWindow> contextToWindow = new Dictionary<IntPtr, NativeWindow>();
        public static Dictionary<VarList, uint> textureLists = new Dictionary<VarList, uint>();
        public static Dictionary<VarList, Bitmap> bitmapLists = new Dictionary<VarList, Bitmap>();
        public static Dictionary<VarList, Font> fontLists = new Dictionary<VarList, Font>();
        public static Dictionary<VarList, uint> shaderLists = new Dictionary<VarList, uint>();
        public static Dictionary<VarList, uint> programLists = new Dictionary<VarList, uint>();
        #endregion


        #region Fake Variables
        public static VarList foregroundColor = new VarList();
        private static uint lastWidth = 0;
        private static uint lastHeight = 0;
        private static int mouseX = 0;
        private static int mouseY = 0;
        private static bool mouseInWindow = false;
        #endregion

        private static int CurrentMouse = 0;

        private static Font currentFont = new Font(new FontFamily("Arial"), 12, FontStyle.Regular, GraphicsUnit.Pixel);
        private static System.Drawing.Text.TextRenderingHint FontAA = System.Drawing.Text.TextRenderingHint.AntiAlias;
        private static uint fontTexture;
        public static Stack<uint> frameBufferStack = new Stack<uint>();
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
                windowList["onMouseMove"] = new VarEvent("onMouseMove");
                windowList["onMouseEnter"] = new VarEvent("onMouseEnter");
                windowList["onMouseLeave"] = new VarEvent("onMouseLeave");
                windowList["onResize"] = new VarEvent("onResize");
                Thread t = null;
                t = new Thread(()=>{
                    using(NativeWindowWinNTCustom nw = new NativeWindowWinNTCustom()){
                        nw.ContextCreated += (object s, NativeWindowEventArgs e)=>{
                            windowList.Get("onLoad").TryCall(new CallData(windowList));
                        };
                        
                        string title = dat.Get(0).Or("title").Otherwise("Funky2").GetString();
                        uint width   = (uint)dat.Get(1).Or("width").Otherwise(640).GetNumber();
                        uint height  = (uint)dat.Get(2).Or("height").Otherwise(480).GetNumber();
                        
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
                        nw.MouseMove += OnMouseMove(nw, windowList);
                        nw.MouseEnter += OnMouseEnter(nw, windowList);
                        nw.MouseLeave += OnMouseLeave(nw, windowList);
                        nw.Resize += OnResize(nw, windowList);
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
            draw["setBlendMode"] = draw["setBlend"] = new VarFunction(dat => {
                string mode = (string)dat.Get(0).Or("mode").Otherwise("OneMinus").GetString();
                switch(mode){
                    case "Screen":
                        Gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor); break;
                    case "Multiply":
                        Gl.BlendFunc(BlendingFactor.Zero, BlendingFactor.SrcColor); break;
                    default:
                        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); break;
                }
                return draw;
            });
            draw["getWidth"] = new VarFunction(dat => lastWidth);
            draw["getHeight"] = new VarFunction(dat => lastHeight);
            draw["getMouseX"] = new VarFunction(dat => mouseX);
            draw["getMouseY"] = new VarFunction(dat => mouseY);
            draw["setCursor"] = new VarFunction(dat => {
                string cursorName = (string)dat.Get(0).Or("cursor").Required().GetString();
                try{
                    CursorShape shape = Enum.Parse<CursorShape>(cursorName, true);
                    if(!mouseInWindow) // Still check the argument, but don't draw if not in game window.
                        return Var.nil;
                    FunkyHelpers.SetCursor(Cursors.Get(shape));
                }catch(ArgumentException){
                    throw new FunkyException($"Invalid Cursor Type {cursorName}");
                }
                return Var.nil;
            });
            draw["loadFont"] = new VarFunction(dat => {
                VarList l = new VarList();
                string family = (string)dat.Get(0).Or("family").Otherwise("Arial").GetString();
                int size      = (int)dat.Get(1).Or("size").Otherwise(12f).GetNumber();
                int AA        = (int)dat.Get(2).Or("AA").Otherwise(1f).GetNumber();
                string style  = dat.Get(3).Or("style").Otherwise("Regular").GetString();
                FontStyle st =  FontStyle.Regular;
                FontStyle.TryParse(style, true, out st);
                Font f = new Font(new FontFamily(family), size, st, GraphicsUnit.Pixel);
                fontLists[l] = f;
                l["AA"] = AA!=0 ? 1 : 0;
                return l;
            });
            draw["setFont"] = new VarFunction(dat => {
                Var font = dat.Get(0).Or("font").Otherwise(Var.nil).Get();
                if(font is VarNull)
                    return draw; // No action
                VarList f = font.asList();
                if(fontLists.ContainsKey(f)){
                    currentFont = fontLists[f];
                    FontAA = f.Get("AA").asBool()?System.Drawing.Text.TextRenderingHint.AntiAlias:System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                }
                return draw;
            });
            draw["loadTexture"] = draw["loadImage"] = new VarFunction(dat => {
                uint text = Gl.GenTexture();
                Gl.BindTexture(TextureTarget.Texture2d, text);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
                Bitmap map = new Bitmap(new FunkyFile(dat.Get(0).Required().GetString(), "images", ".png", ".jpg", ".bmp").realPath);
                BitmapData bm = null;
                try{
                    bm = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadOnly, map.PixelFormat);
                    Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, map.Width, map.Height, 0, map.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? OpenGL.PixelFormat.Bgr : OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bm.Scan0);
                }finally{
                    if(bm != null)
                        map.UnlockBits(bm);
                }
                VarList l = new VarList();
                textureLists[l] = text;
                bitmapLists[l] = map;
                bool destroyed = false;
                l["getWidth"] = new VarFunction(dat =>{
                    if(destroyed)return Var.nil;
                    return map.Width;
                });
                l["getHeight"] = new VarFunction(dat => {
                    if(destroyed)return Var.nil;
                    return map.Height;
                });
                l["destroy"] = new VarFunction(dat =>{
                    if(destroyed)return Var.nil;
                    destroyed = true;
                    Gl.DeleteTextures(text);
                    textureLists.Remove(l);
                    bitmapLists.Remove(l);
                    map.Dispose();
                    return draw;
                });
                Gl.BindTexture(TextureTarget.Texture2d, 0);
                //Gl.GenerateMipmap(TextureTarget.Texture2d);
                return l;
            });
            draw["texturedRect"] = draw["image"] = new VarFunction(dat => {
                VarList l = dat.Get(0).Or("tex").Required().GetList();
                if(textureLists.ContainsKey(l)){
                    uint text = textureLists[l];
                    double x  = (double)dat.Get(1).Or("x").Otherwise(0.0d).GetNumber();
                    double y  = (double)dat.Get(2).Or("y").Otherwise(0.0d).GetNumber();
                    double r  = (double)dat.Get(3).Or("r").Otherwise(0.0d).GetNumber();
                    double sx = (double)dat.Get(4).Or("sx").Otherwise(1.0d).GetNumber();
                    double sy = (double)dat.Get(5).Or("sy").Otherwise(1.0d).GetNumber();
                    double ox = (double)dat.Get(6).Or("ox").Otherwise(0.0d).GetNumber();
                    double oy = (double)dat.Get(7).Or("oy").Otherwise(0.0d).GetNumber();
                    double kx = (double)dat.Get(8).Or("kx").Otherwise(1.0d).GetNumber();
                    double ky = (double)dat.Get(9).Or("ky").Otherwise(1.0d).GetNumber();

                    int w = (int)l.Get("getWidth").TryCall(new CallData(l)).asNumber();
                    int h = (int)l.Get("getHeight").TryCall(new CallData(l)).asNumber();

                    Gl.PushMatrix();
                        Gl.Translate(x, y, 0d);
                        Gl.Scale(sx, sy, 0d);
                        Gl.Rotate(r, 0.0d, 0.0d, 1.0d);
                        Gl.Translate(-ox*w, -oy*h, 0d);
                        Gl.BindTexture(TextureTarget.Texture2d, text);
                        Gl.Begin(PrimitiveType.Quads);
                            Gl.TexCoord2(0f, 0f);
                            Gl.Vertex2(0d, 0d);
                            Gl.TexCoord2((float)kx, 0f);
                            Gl.Vertex2(w, 0d);
                            Gl.TexCoord2((float)kx, (float)ky);
                            Gl.Vertex2(w, h);
                            Gl.TexCoord2(0f, (float)ky);
                            Gl.Vertex2(0d, h);
                        Gl.End();
                    Gl.PopMatrix();
                    Gl.BindTexture(TextureTarget.Texture2d, 0);
                }else{
                    return draw;
                }
                return draw;
            });
            draw["push"] = new VarFunction(dat => {Gl.PushMatrix(); return draw;});
            draw["pop"] = new VarFunction(dat => {Gl.PopMatrix(); return draw;});
            draw["translate"] = new VarFunction(dat => {
                double x = (double)dat.Get(0).Or("x").Otherwise(0.0d).GetNumber();
                double y = (double)dat.Get(1).Or("y").Otherwise(0.0d).GetNumber();
                Gl.Translate(x, y, 0.0d);
                return draw;
            });
            draw["rotate"] = new VarFunction(dat => {
                double r = (double)dat.Get(0).Or("r").Otherwise(0.0d).GetNumber();
                Gl.Rotate(r, 0.0d, 0.0d, 1.0d);
                return draw;
            });
            draw["scale"] = new VarFunction(dat => {
                double x = (double)dat.Get(0).Or("x").Otherwise(0.0d).GetNumber();
                double y = (double)dat.Get(1).Or("y").Otherwise(0.0d).GetNumber();
                Gl.Scale(x, y, 0.0d);
                return draw;
            });
            draw["rect"] = draw["rectangle"] = draw["box"] = new VarFunction(dat => {
                double x = (double)dat.Get(0).Or("x").Otherwise(0.0d).GetNumber();
                double y = (double)dat.Get(1).Or("y").Otherwise(0.0d).GetNumber();
                double w = (double)dat.Get(2).Or("w").Otherwise(0.0d).GetNumber();
                double h = (double)dat.Get(3).Or("h").Otherwise(0.0d).GetNumber();

                Gl.Begin(PrimitiveType.Quads);
                    Gl.TexCoord2(0, 0f);
                    Gl.Vertex2(x, y);
                    Gl.TexCoord2(w, 0f);
                    Gl.Vertex2(x+w, y);
                    Gl.TexCoord2(w, h);
                    Gl.Vertex2(x+w, y+h);
                    Gl.TexCoord2(0f, h);
                    Gl.Vertex2(x, y+h);
                Gl.End();

                return draw;
            });
            draw["poly"] = draw["polygon"] = new VarFunction(dat => {
                // Ensure 6 arguments.
                for(int i=0; i < 6; i++)if(!dat._num_args.ContainsKey(i))return Var.nil;

                Gl.Begin(PrimitiveType.Polygon);
                    for(int i=0; dat._num_args.ContainsKey(i) && dat._num_args.ContainsKey(i+1); i+=2){
                        Gl.TexCoord2(dat._num_args[i].asNumber(), dat._num_args[i+1].asNumber());
                        Gl.Vertex2(dat._num_args[i].asNumber(), dat._num_args[i+1].asNumber());
                    }
                Gl.End();

                return draw;
            });
            draw["text"] = draw["print"] = new VarFunction(dat => {
                string showText = (string)dat.Get(0).Or("text").Otherwise("").GetString();
                float x         = (float)dat.Get(1).Or("x").Otherwise(0f).GetNumber();
                float y         = (float)dat.Get(2).Or("y").Otherwise(0f).GetNumber();
                int ha          = (int)dat.Get(3).Or("ha").Otherwise(-1f).GetNumber();
                int va          = (int)dat.Get(4).Or("va").Otherwise(-1f).GetNumber();
                float ox        = (float)dat.Get(5).Or("ox").Otherwise(0f).GetNumber();
                float oy        = (float)dat.Get(6).Or("oy").Otherwise(0f).GetNumber();

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
                map.Dispose();
                return draw;
            });
            draw["textSize"] = new VarFunction(dat => {
                var textToMeasure = dat.Get(0).Or("text").Required().GetString();
                SizeF textSize = Graphics.FromImage(new Bitmap(1,1)).MeasureString(textToMeasure, currentFont);
                VarList outData = new VarList();
                outData["width"] = textSize.Width;
                outData["height"] = textSize.Height;
                outData["text"] = textToMeasure;
                return outData;
            });
            draw["setColor"] = new VarFunction(dat => {
                foregroundColor.double_vars[0] = (float)dat.Get(0).Or("r").Otherwise(0.0f).GetNumber();
                foregroundColor.double_vars[1] = (float)dat.Get(1).Or("g").Otherwise(0.0f).GetNumber();
                foregroundColor.double_vars[2] = (float)dat.Get(2).Or("b").Otherwise(0.0f).GetNumber();
                foregroundColor.double_vars[3] = (float)dat.Get(3).Or("a").Otherwise(1.0f).GetNumber();
                Gl.Color4((float)foregroundColor.double_vars[0].asNumber(), (float)foregroundColor.double_vars[1].asNumber(), (float)foregroundColor.double_vars[2].asNumber(), (float)foregroundColor.double_vars[3].asNumber());
                return foregroundColor;
            });
            draw["clear"] = new VarFunction(dat => {
                Gl.Clear(ClearBufferMask.ColorBufferBit);
                return draw;
            });
            draw["createShader"] = new VarFunction(dat => {
                string source       = dat.Get(0).Or("source").Otherwise("").GetString();
                string shadertype   = dat.Get(1).Or("type").Otherwise("frag").GetString();
                if(source == "")
                    return Var.nil; // No source??!?!?
                ShaderType typ = shadertype.ToLower().Substring(0, 4) == "frag" ? ShaderType.FragmentShader : ShaderType.VertexShader;

                uint shader = Gl.CreateShader(typ);
                Gl.ShaderSource(shader, source.Split('\n').Select(x=>x.Replace('\r',' ') +"\n").ToArray());
                Gl.CompileShader(shader);
                int compiled;

	            Gl.GetShader(shader, ShaderParameterName.CompileStatus, out compiled);
                if(compiled != 0){
                    VarList shaderList = new VarList();
                    shaderLists[shaderList] = shader;
                    return shaderList;
                }
                const int logMaxLength = 1024;
                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;
                Gl.GetShaderInfoLog(shader, logMaxLength, out infologLength, infolog);
                Gl.DeleteShader(shader);

                return infolog.ToString();
            });
            draw["useShaders"] = draw["useShader"] = new VarFunction(dat => {
                Var shad = dat.Get(0).Or("shaders").Otherwise(Var.nil).Get();
                if(shad is VarNull){
                    Gl.UseProgram(0);
                    return draw; // No action
                }
                VarList s = shad.asList();
                if(programLists.ContainsKey(s)){
                    Gl.UseProgram(programLists[s]);
                }
                return draw;
            });
            draw["createShaderScheme"] = draw["createProgram"] = draw["makeShaders"] = new VarFunction(dat => {
                uint prog = Gl.CreateProgram();
                VarList pList = new VarList();
                programLists[pList] = prog;

                for(int i=0; dat._num_args.ContainsKey(i); i++){
                    VarList l = dat._num_args[i].asList();
                    if(shaderLists.ContainsKey(l))
                        Gl.AttachShader(prog, shaderLists[l]);
                }
                Gl.LinkProgram(prog);

                return pList;
            });
            /*draw["sendToProgram"] = new VarFunction(dat => {
                var program = dat.Get(0).Or("program").Required().GetList();
                var name = dat.Get(1).Or("name").Required().GetString();
                var value = dat.Get(2).Or("value").Otherwise(Var.nil).Get();

                if(!programLists.ContainsKey(program)){
                    return Var.nil;
                }
                uint programID = programLists[program];
                int uniformLocation = Gl.GetUniformLocation(programID, name);
                Gl.Uniform1(uniformLocation, value.asNumber());
            });*/
            draw["createCanvas"] = draw["newCanvas"] = new VarFunction(dat => {
                int w = (int)dat.Get(0).Or("w").Otherwise(lastWidth).GetNumber();
                int h = (int)dat.Get(1).Or("h").Otherwise(lastHeight).GetNumber();
                VarList canvList = new VarList();

                uint canvasTexture = Gl.GenTexture();
                Gl.BindTexture(TextureTarget.Texture2d, canvasTexture);
                Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, w, h, 0, OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, new IntPtr(0));
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
                Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);

                textureLists[canvList] = canvasTexture;
                bool destroyed = false;
                canvList["getWidth"] = new VarFunction(dat => {
                    if(destroyed)return Var.nil;
                    return w;
                });
                canvList["getHeight"] = new VarFunction(dat => {
                    if(destroyed)return Var.nil;
                    return h;
                });

                uint frameBuffer = Gl.GenFramebuffer();
                Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBuffer);
                Gl.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, canvasTexture, 0);
                Gl.BindTexture(TextureTarget.Texture2d, 0);
                
                Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                canvList["drawTo"] = new VarFunction(d => {
                    if(destroyed)return Var.nil;
                    uint oldWidth = lastWidth;
                    uint oldHeight = lastHeight;
                    lastWidth = (uint)w;
                    lastHeight = (uint)h;
                    VarFunction f = d.Get(0).Or("drawFunc").Otherwise(Var.nil).GetFunction();
                    Gl.PushMatrix();
                        Gl.Viewport(0, 0, w, h);
                        Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBuffer);
                        frameBufferStack.Push(frameBuffer);
                        Gl.Clear(ClearBufferMask.ColorBufferBit);
                        Gl.MatrixMode(MatrixMode.Projection);
                        Gl.PushMatrix();
                            Gl.LoadIdentity();
                            Gl.Ortho(0.0, w, 0.0, h, 0.0, 1.0);
                            Gl.MatrixMode(MatrixMode.Modelview);
                            Gl.LoadIdentity();
                            f.TryCall(new CallData(canvList));
                            Gl.MatrixMode(MatrixMode.Projection);
                        Gl.PopMatrix();
                        Gl.MatrixMode(MatrixMode.Modelview);
                        frameBufferStack.Pop();
                        if(frameBufferStack.Count == 0)
                            Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                        else
                            Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBufferStack.Peek());

                        Gl.Viewport(0, 0, (int)oldWidth, (int)oldHeight);
                    Gl.PopMatrix();
                    lastWidth = oldWidth;
                    lastHeight = oldHeight;
                    return canvList;
                });
                canvList["destroy"] = new VarFunction(d => {
                    if(destroyed)return Var.nil;
                    destroyed = true;
                    Gl.DeleteFramebuffers(frameBuffer);
                    Gl.DeleteTextures(canvasTexture);

                    textureLists.Remove(canvList);

                    return draw;
                });
                return canvList;
            });
            draw["newImageData"] = new VarFunction(dat => {
                VarList dataList = new VarList();
                Var filename = dat.Get(0).Or("file").Otherwise(Var.nil).Get();
                int w           = (int)dat.Get(0).Or("w").Otherwise(1).GetNumber();
                int h           = (int)dat.Get(1).Or("h").Otherwise(1).GetNumber();
                w = Math.Max(w, 1);
                h = Math.Max(h, 1);
                Bitmap map;
                if(filename is VarString){
                    map = new Bitmap(new FunkyFile(filename.asString(), "images", ".png", ".jpg", ".bmp").realPath);
                }else{
                    map = new Bitmap(w, h);
                }
                bool destroyed = false;
                dataList["getWidth"] = new VarFunction(dat => map.Width);
                dataList["getHeight"] = new VarFunction(dat => map.Height);
                dataList["destroy"] = new VarFunction(d => {
                    if(destroyed)return Var.nil;
                    destroyed = true;
                    map.Dispose();
                    bitmapLists.Remove(dataList);
                    return draw;
                });
                dataList["getPixel"] = new VarFunction(d => {
                    if(destroyed)return Var.nil;
                    int x           = (int)d.Get(0).Or("x").Otherwise(0).GetNumber();
                    int y           = (int)d.Get(1).Or("y").Otherwise(0).GetNumber();
                    x = (int)Math.Clamp(x, 0, map.Width);
                    y = (int)Math.Clamp(y, 0, map.Height);
                    int col = map.GetPixel(x, y).ToArgb();
                    int a = (col>>3*8)&0xFF;
                    int r = (col>>2*8)&0xFF;
                    int g = (col>>1*8)&0xFF;
                    int b = (col>>0*8)&0xFF;
                    VarList oList = new VarList();
                    oList[0] = oList["r"] = ((float)r)/255;
                    oList[1] = oList["g"] = ((float)g)/255;
                    oList[2] = oList["b"] = ((float)b)/255;
                    oList[3] = oList["a"] = ((float)a)/255;
                    return oList;
                });
                dataList["setPixel"] = new VarFunction(d => {
                    if(destroyed)return Var.nil;
                    int x           = (int)d.Get(0).Or("x").Otherwise(0).GetNumber();
                    int y           = (int)d.Get(1).Or("y").Otherwise(0).GetNumber();
                    VarList rgba    =      d.Get(2).Or("color").Otherwise(Var.nil).GetList();
                    float r = rgba.string_vars.ContainsKey("r") ? (float)rgba["r"].asNumber()
                            : rgba.double_vars.ContainsKey(0)   ? (float)rgba[ 0 ].asNumber()
                            : 1.0f;
                    float g = rgba.string_vars.ContainsKey("g") ? (float)rgba["g"].asNumber()
                            : rgba.double_vars.ContainsKey(1)   ? (float)rgba[ 1 ].asNumber()
                            : 1.0f;
                    float b = rgba.string_vars.ContainsKey("b") ? (float)rgba["b"].asNumber()
                            : rgba.double_vars.ContainsKey(2)   ? (float)rgba[ 2 ].asNumber()
                            : 1.0f;
                    float a = rgba.string_vars.ContainsKey("a") ? (float)rgba["a"].asNumber()
                            : rgba.double_vars.ContainsKey(3)   ? (float)rgba[ 3 ].asNumber()
                            : 1.0f;
                    r = Math.Clamp(r, 0.0f, 1.0f);
                    g = Math.Clamp(g, 0.0f, 1.0f);
                    b = Math.Clamp(b, 0.0f, 1.0f);
                    a = Math.Clamp(a, 0.0f, 1.0f);
                    x = (int)Math.Clamp(x, 0, map.Width);
                    y = (int)Math.Clamp(y, 0, map.Height);
                    map.SetPixel(x, y, Color.FromArgb((int)(255*a), (int)(255*r), (int)(255*g), (int)(255*b)));
                    return dataList;
                });
                dataList["mapPixels"] = new VarFunction(d => {
                    if(destroyed)return Var.nil;
                    VarFunction action = d.Get(0).Or("action").Otherwise(Var.nil).GetFunction();
                    for(int y=0; y<map.Height; y++){
                        for(int x=0; x < map.Width; x++){
                            int col = map.GetPixel(x, y).ToArgb();
                            int a = (col>>3*8)&0xFF;
                            int r = (col>>2*8)&0xFF;
                            int g = (col>>1*8)&0xFF;
                            int b = (col>>0*8)&0xFF;
                            VarList cList = new VarList();
                            cList[0] = cList["r"] = ((float)r)/255;
                            cList[1] = cList["g"] = ((float)g)/255;
                            cList[2] = cList["b"] = ((float)b)/255;
                            cList[3] = cList["a"] = ((float)a)/255;
                            CallData cd = new CallData(x, y, cList);
                            cd._str_args["x"] = x;
                            cd._str_args["y"] = y;
                            cd._str_args["col"] = cList;
                            VarList rgba = action.TryCall(cd).asList();
                            float R = rgba.string_vars.ContainsKey("r") ? (float)rgba["r"].asNumber()
                                    : rgba.double_vars.ContainsKey(0)   ? (float)rgba[ 0 ].asNumber()
                                    : 1.0f;
                            float G = rgba.string_vars.ContainsKey("g") ? (float)rgba["g"].asNumber()
                                    : rgba.double_vars.ContainsKey(1)   ? (float)rgba[ 1 ].asNumber()
                                    : 1.0f;
                            float B = rgba.string_vars.ContainsKey("b") ? (float)rgba["b"].asNumber()
                                    : rgba.double_vars.ContainsKey(2)   ? (float)rgba[ 2 ].asNumber()
                                    : 1.0f;
                            float A = rgba.string_vars.ContainsKey("a") ? (float)rgba["a"].asNumber()
                                    : rgba.double_vars.ContainsKey(3)   ? (float)rgba[ 3 ].asNumber()
                                    : 1.0f;
                            R = Math.Clamp(R, 0.0f, 1.0f);
                            G = Math.Clamp(G, 0.0f, 1.0f);
                            B = Math.Clamp(B, 0.0f, 1.0f);
                            A = Math.Clamp(A, 0.0f, 1.0f);
                            map.SetPixel(x, y, Color.FromArgb((int)(255*A), (int)(255*R), (int)(255*G), (int)(255*B)));
                        }
                    }
                    return dataList;
                });
                dataList["toImage"] = new VarFunction(d => {
                    uint text = Gl.GenTexture();
                    Gl.BindTexture(TextureTarget.Texture2d, text);
                    Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
                    Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
                    BitmapData bm = null;
                    try{
                        bm = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadOnly, map.PixelFormat);
                        Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, map.Width, map.Height, 0, map.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? OpenGL.PixelFormat.Bgr : OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bm.Scan0);
                    }finally{
                        if(bm != null)
                            map.UnlockBits(bm);
                    }
                    VarList l = new VarList();
                    textureLists[l] = text;
                    bitmapLists[l] = map;
                    bool destroyed = false;
                    l["getWidth"] = new VarFunction(dat =>{
                        if(destroyed)return Var.nil;
                        return map.Width;
                    });
                    l["getHeight"] = new VarFunction(dat => {
                        if(destroyed)return Var.nil;
                        return map.Height;
                    });
                    l["destroy"] = new VarFunction(dat =>{
                        if(destroyed)return Var.nil;
                        destroyed = true;
                        Gl.DeleteTextures(text);
                        textureLists.Remove(l);
                        bitmapLists.Remove(l);
                        return draw;
                    });
                    Gl.BindTexture(TextureTarget.Texture2d, 0);
                    //Gl.GenerateMipmap(TextureTarget.Texture2d);
                    return l;
                });
                dataList["export"] = new VarFunction(d => {
                    string filename = d.Get(0).Or("file").Or("filename").Required().GetString();
                    FunkyFile f = new FunkyFile(filename, "images", ".png", ".jpg", ".bmp");
                    map.Save(f.realPath??filename);
                    return filename;
                });
                bitmapLists[dataList] = map;
                return dataList;
            });
            return draw;
        }

        #region Window Events
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

                l.Get("onDraw").TryCall(new CallData(l));
            };
        }

        public static EventHandler<NativeWindowKeyEventArgs> OnKeyDown(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowKeyEventArgs e)=>{
                int keyCode = (int)e.Key;
                string keyName = CodeToKey(e.Key);
                CallData cd = new CallData(keyCode, keyName);
                cd._str_args["code"] = keyCode;
                cd._str_args["key"] = keyName;
                l.Get("onKeyDown").TryCall(cd);
            };
        }
        public static EventHandler<NativeWindowKeyEventArgs> OnKeyUp(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowKeyEventArgs e)=>{
                int keyCode = (int)e.Key;
                string keyName = CodeToKey(e.Key);
                CallData cd = new CallData(keyCode, keyName);
                cd._str_args["code"] = keyCode;
                cd._str_args["key"] = keyName;
                l.Get("onKeyUp").TryCall(cd);
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
                        CallData cd = new CallData(button, x, ((int)lastHeight)-y);
                        cd._str_args["button"] = button;
                        cd._str_args["x"] = x;
                        cd._str_args["y"] = ((int)lastHeight)-y;
                        l.Get("onMouseDown").TryCall(cd);
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
                        CallData cd = new CallData(button, x, ((int)lastHeight)-y);
                        cd._str_args["button"] = button;
                        cd._str_args["x"] = x;
                        cd._str_args["y"] = ((int)lastHeight)-y;
                        l.Get("onMouseUp").TryCall(cd);
                    }
                }    
            };
        }
        public static EventHandler<NativeWindowMouseEventArgsWorking> OnMouseWheel(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowMouseEventArgsWorking e)=>{
                int delta = e.WheelTicks;
                var x = e.Location.X;
                var y = e.Location.Y;
                CallData cd = new CallData(delta, x, ((int)lastHeight)-y);
                cd._str_args["delta"] = delta;
                cd._str_args["x"] = x;
                cd._str_args["y"] = ((int)lastHeight)-y;
                l.Get("onMouseWheel").TryCall(cd); 
            };
        }
        public static EventHandler<NativeWindowMouseEventArgs> OnMouseMove(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowMouseEventArgs e)=>{
                var x = e.Location.X;
                var y = e.Location.Y;
                mouseX = x;
                mouseY = ((int)lastHeight)-y;
                CallData cd = new CallData(x, ((int)lastHeight)-y);
                cd._str_args["x"] = x;
                cd._str_args["y"] = ((int)lastHeight)-y;
                l.Get("onMouseMove").TryCall(cd); 
            };
        }
        public static EventHandler<NativeWindowMouseEventArgs> OnMouseEnter(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowMouseEventArgs e)=>{
                mouseInWindow = true;
                var x = e.Location.X;
                var y = e.Location.Y;
                mouseX = x;
                mouseY = ((int)lastHeight)-y;
                CallData cd = new CallData(x, ((int)lastHeight)-y);
                cd._str_args["x"] = x;
                cd._str_args["y"] = ((int)lastHeight)-y;
                l.Get("onMouseEnter").TryCall(cd); 
            };
        }
        public static EventHandler<NativeWindowEventArgs> OnMouseLeave(NativeWindow window, VarList l){
            return (object c_Sender, NativeWindowEventArgs e)=>{
                mouseInWindow = false;
                CallData cd = new CallData();
                l.Get("onMouseLeave").TryCall(cd); 
            };
        }

        public static EventHandler<EventArgs> OnResize(NativeWindow window, VarList l){
            return (object c_Sender, EventArgs e)=>{
                int oldW = (int)lastWidth;
                int oldH = (int)lastHeight;
                int w = (int)(lastWidth = window.Width);
                int h = (int)(lastHeight = window.Height);
                CallData cd = new CallData(w,h,oldW,oldH);
                cd._str_args["w"] = w;
                cd._str_args["h"] = h;
                cd._str_args["oldW"] = oldW;
                cd._str_args["oldH"] = oldH;
                l.Get("onResize").TryCall(cd); 
            };
        }
        #endregion

        public static VarList WindowMeta(NativeWindow window, VarList l){
            VarList meta = new VarList();
            VarList metafuncs = new VarList();

            metafuncs["isKeyDown"] = new VarFunction(d=>{
                Var key = d.Get(0).Required().Get();

                if(key is VarString)
                    return window.IsKeyPressed(KeyToCode(key.asString()))?1:0;
                else if(key is VarNumber)
                    return window.IsKeyPressed((KeyCode)key.asNumber().value)?1:0;
                return Var.nil;
            });

            metafuncs["setTitle"] = new VarFunction(d=>{
                Var a = d.Get(0).Or("title").Otherwise(Var.nil).Get();
                if(a != Var.nil)
                    FunkyHelpers.SetWindowTextW(window.Handle, a.asString());
                return a;
            });

            meta["get"] = new VarFunction(d=>{
                return metafuncs[d.Get(1).Required().Get()];
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