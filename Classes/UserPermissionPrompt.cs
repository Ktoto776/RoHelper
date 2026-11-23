using MoonSharp.Interpreter;

namespace RoHelper;

class UserPermissionPromptForm : Form
{
    public EventHandler? Decline;
    public EventHandler? Accept;
    public bool isChoiced = false;
    public string UserPermissionName ="";
    private TableLayoutPanel mainTableLayout = new(){
        RowCount = 4,
        ColumnCount = 1,
        Padding = new(0),
        Margin = new(0),
        BackColor = Color.Gray
    };
    private Label ScriptRequresLabel =new(){
        Text="YouWantToGrantPermission",
        TextAlign=ContentAlignment.MiddleCenter
    };
    private TableLayoutPanel buttonsTableLayout = new(){
        ColumnCount = 2,
        RowCount = 1,
        // Padding = new(0),
        // Margin = new(0),
        BackColor = Color.DarkGray
    };
    public UserPermissionPromptForm(ScriptMethods methods)
    {
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        TopMost = true;
        var width = (int)(500*UserPermissionPrompt.FormScale);
        var height = (int)(300*UserPermissionPrompt.FormScale);
        Size = new Size(width,height);
        // UI
        Controls.Add(mainTableLayout);
        mainTableLayout.Dock = DockStyle.Fill;
            // ScriptRequresLabel
        mainTableLayout.RowStyles.Add(new(SizeType.AutoSize));
        mainTableLayout.Controls.Add(ScriptRequresLabel,0,0);
        ScriptRequresLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        Program.translator.bindToTranslateChanged(ScriptRequresLabel.Text,(txt)=>{
            ScriptRequresLabel.Text = txt.Replace("<name>",methods.DisplayName);
        });
        mainTableLayout.RowStyles.Add(new(SizeType.AutoSize));
        mainTableLayout.RowStyles.Add(new(SizeType.AutoSize));
        mainTableLayout.RowStyles.Add(new(SizeType.Percent,1f));
        // mainTableLayout.ColumnStyles[mainTableLayout.ColumnCount-1] = ;
        mainTableLayout.Controls.Add(buttonsTableLayout,0,mainTableLayout.RowCount-1);
        buttonsTableLayout.Dock = DockStyle.Fill;
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
    public UserPermissionPromptForm Form {get; private set;}
    public string UserPermissionName{
        get=>Form.UserPermissionName;
        set=>Form.UserPermissionName=value;
    }
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
        Form = new(Script.scriptMethods);
        Program.translator.bindToTranslator(RefreshFromText);
        Program.mainWindow.Invoke(new Action(Form.Show));
    }
}