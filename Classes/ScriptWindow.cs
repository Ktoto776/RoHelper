using System.Reflection;
using MoonSharp.Interpreter;

namespace RoHelper;

class ScriptWindowForm : Form
{
    private ScriptWindow scriptWindow;
    private string? SaveName;
    public void refreshBorderFixedStyle(){
        if (scriptWindow.isBorderFixed||(MaximumSize.Equals(MinimumSize)&& MaximumSize != Size.Empty))
            FormBorderStyle = FormBorderStyle.FixedSingle;
        else FormBorderStyle = FormBorderStyle.Sizable;
    }
    public void refreshMaximazeBox(){
        MaximizeBox = (MaximumSize==Size.Empty)&scriptWindow.MaximazeBox;
    }
    public ScriptWindowForm(ScriptWindow scriptWindow,string? SaveName =null){
        this.SaveName = SaveName;
        this.scriptWindow = scriptWindow;
        MinimumSizeChanged += (_,_) =>refreshBorderFixedStyle();
        MaximumSizeChanged += (_, _) =>{ 
            refreshMaximazeBox();
            refreshBorderFixedStyle();
        }; using (Stream? iconStream = Assembly.GetExecutingAssembly().
            GetManifestResourceStream("RoHelper.resources.icon.ico")){
            if (iconStream != null){
                Icon = new Icon(iconStream);
            }
        }
    }
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Save
        if(SaveName!=null){
            var data = Program.appData.getWindowData(SaveName);
            if (data!=null){
                Size = new Size(data.Width,data.Height);
                Location = new Point(data.X,data.Y);
            }
        }
    }
    public EventHandler? ClosingEvent;
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        this.Hide();
        if (e.CloseReason == CloseReason.UserClosing){
            e.Cancel = true;
        }else{ ClosingEvent?.Invoke(this,EventArgs.Empty);
            if(SaveName!=null){
                var data = new AppData_WindowSave();
                data.X = this.Location.X;
                data.Y = this.Location.Y;
                data.Width = this.Size.Width;
                data.Height = this.Size.Height;
                Program.appData.WindowData[SaveName] = data;
            }
        } base.OnFormClosing(e);
    }
}

class ScriptWindow
{
    [MoonSharpHidden]
    public ScriptMethods methods;
    private ScriptWindowForm form;
    public LuaScript.LuaEvent VisibleChanged;
    public bool isVisible{
        get=>form.Visible;
        set{ if (value) Show();
            else Hide();
        }
    }
    private bool _isDestroyed = false;
    public LuaScript.LuaEvent Destroying;
    public DynValue Destroy(){
        if (_isDestroyed) return DynValue.FromObject(methods.script.luaScript,
            new CSharpLuaError("Error to destroy window. Window is already destroyed."));
        Destroying.trigger();
        Program.mainWindow.Invoke(new Action(form.Close));
        _isDestroyed = true;
        return DynValue.Nil;
    }
    private bool _firstOpen = true;
    public DynValue Show(){
        if (_isDestroyed) return DynValue.FromObject(methods.script.luaScript,
            new CSharpLuaError("Error to show window. Window is destroyed."));
        if (_firstOpen){
            _firstOpen = false;
            Program.mainWindow.Invoke(new Action(form.Show));
        } Program.mainWindow.Invoke(new Action(()=>{
            form.Visible = true; form.Focus();
        })); return DynValue.Nil;
    }
    private void triggerVisibleChanged(){
        if (_isDestroyed) return;
        if (form.Visible) VisibleChanged.trigger([DynValue.True]);
            else VisibleChanged.trigger([DynValue.False]);
    }
    public DynValue Hide(){
        if (_isDestroyed) return DynValue.FromObject(methods.script.luaScript,
            new CSharpLuaError("Error to hide window. Window is destroyed."));
        Program.mainWindow.Invoke(new Action(()=>form.Visible = false));
        return DynValue.Nil;
    }
    private bool _isBorderFixed =false;
    public bool isBorderFixed{
        get=>_isBorderFixed;
        set{ _isBorderFixed=value;
            Program.mainWindow.Invoke(new Action(form.refreshBorderFixedStyle));
        }
    }
    private bool _MaximazeBox =false;
    public bool MaximazeBox{
        get=>_MaximazeBox;
        set{ _MaximazeBox=value;
            Program.mainWindow.Invoke(new Action(form.refreshMaximazeBox));
        }
    }
    private readonly PixelsSize _Size =new();
    public PixelsSize Size{
        get=>_Size;
        set=>_Size.CloneFrom(value);
    } private readonly PixelsSize _MinimumSize =new();
    public PixelsSize MinimumSize{
        get=>_MinimumSize;
        set=>_MinimumSize.CloneFrom(value);
    } private readonly PixelsSize _MaximumSize =new();
    public PixelsSize MaximumSize{
        get=>_MaximumSize;
        set=>_MaximumSize.CloneFrom(value);
    }
    public PixelsSize ClientSize{
        get=>new(form.ClientSize.Width,form.ClientSize.Height);
    }
    public LuaScript.LuaEvent ClientSizeChanged;
    private string SaveName;
    public string Name;
    public LuaScript.LuaEvent SizeChanged;
    public LuaScript.LuaEvent MaximumSizeChanged;
    public LuaScript.LuaEvent MinimumSizeChanged;
    public ScriptWindow(ScriptMethods methods,string Name){
        this.methods = methods;
        this.Name = Name;
        Destroying = new(methods.script);
        VisibleChanged = new(methods.script);
        SizeChanged = new(methods.script);
        MaximumSizeChanged = new(methods.script);
        MinimumSizeChanged = new(methods.script);
        ClientSizeChanged = new(methods.script);
        SaveName = methods.script.scriptName;
        if (Name!="") SaveName = SaveName+"_-_"+Name;
        form = new(this,SaveName);
        form.ClientSizeChanged += (_,_)=>ClientSizeChanged.trigger();
        _Size.RawSet(form.Width,form.Height);
        _MinimumSize.RawSet(form.MinimumSize.Width,form.MinimumSize.Height);
        _MaximumSize.RawSet(form.MaximumSize.Width,form.MaximumSize.Height);
        form.ClosingEvent += (_,_)=>{
            Destroying.trigger();
            _isDestroyed = true;
        }; form.SizeChanged += (_,_)=>{
            _Size.RawSet(form.Width,form.Height);
            SizeChanged.trigger();
        }; form.VisibleChanged += (_,_)=>triggerVisibleChanged();
        _Size.changedEvent += (_,_)=>
            Program.mainWindow.Invoke(new Action(()=>{
                form.Width = _Size.Width;
                form.Height = _Size.Height;
            })); 
        form.MinimumSizeChanged += (_,_)=>
            _MinimumSize.RawSet(form.MinimumSize.Width,form.MinimumSize.Height);
        _MinimumSize.changedEvent += (_,_)=>{
            Program.mainWindow.Invoke(new Action(()=>
                form.MinimumSize = new(_MinimumSize.Width,_MinimumSize.Height)
            )); MinimumSizeChanged.trigger();
        };
        form.MaximumSizeChanged += (_,_)=>
            _MaximumSize.RawSet(form.MaximumSize.Width,form.MaximumSize.Height);
        _MaximumSize.changedEvent += (_,_)=>{
            Program.mainWindow.Invoke(new Action(()=>
                form.MaximumSize = new(_MaximumSize.Width,_MaximumSize.Height)
            )); MaximumSizeChanged.trigger();
        };
    }
}