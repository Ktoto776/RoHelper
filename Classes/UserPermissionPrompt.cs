namespace RoHelper;

class UserPermissionPromptForm : Form
{
    public EventHandler? Decline;
    public EventHandler? Accept;
    public bool isChoiced = false;
    public UserPermissionPromptForm()
    {
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        TopMost = true;
        var width = (int)(500*UserPermissionPrompt.FormScale);
        var height = (int)(250*UserPermissionPrompt.FormScale);
        Size = new Size(width,height);
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!isChoiced) Decline?.Invoke(this,EventArgs.Empty);
        base.OnFormClosing(e);
    }
}

class UserPermissionPrompt
{
    public static double FormScale = 1;
    public UserPermissionPromptForm Form {get; private set;} = new();
    private LuaScript _script;
    public LuaScript Script{
        get=>_script;
        set{
            _script = value;
            RefreshFromText();
        }
    }
    private void RefreshFromText(){
        Program.mainWindow.Invoke(()=>{
            Form.Text = Program.translator.getTranslated("PermissionRequestFrom")
                +" '"+_script.scriptName+"' [RoHelper]";
        });
    }
    public UserPermissionPrompt(LuaScript Script){
        _script = Script;
        Program.translator.bindToTranslator(RefreshFromText);
        Program.mainWindow.Invoke(new Action(Form.Show));
    }
}