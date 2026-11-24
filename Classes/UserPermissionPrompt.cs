using MoonSharp.Interpreter;
using System.ComponentModel;
using System.Media;

namespace RoHelper;

class UserPermissionPromptForm : Form
{
    public EventHandler? Decline;
    public EventHandler? Accept;
    public EventHandler? AcceptOnce;
    public bool isChoiced = false;
    public string _userPermissionName = "";
    private TableLayoutPanel mainTableLayout = new(){
        RowCount = 4,
        ColumnCount = 1,
        Padding = new(0),
        Margin = new(0),
    };
    private Label ScriptRequresLabel =new(){
        Text="YouWantToGrantPermission",
        AutoSize=true,
        TextAlign=ContentAlignment.MiddleCenter
    };
    private Label PermNameLabel =new(){
        AutoSize=true,
        TextAlign=ContentAlignment.MiddleCenter
    };
    private Label DescriptionLabel =new(){
        TextAlign=ContentAlignment.TopCenter,
        Dock = DockStyle.Fill
    };
    private TableLayoutPanel buttonsTableLayout = new(){
        ColumnCount = 3,
        RowCount = 1,
        Padding = new(0),
        // Margin = new(0),
        Dock=DockStyle.Fill,
    };
    private Button DeclineButton = new(){
        Text="declinePermission",
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink
    };
    private Button OnceButton = new(){
        Text="allowOncePermission",
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink
    };
    private Button AllowButton = new(){
        Text="allowPermission",
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink
    };
    public void refreshPermissionLabels(){
        PermNameLabel.Text = Program.translator.getTranslated(
            "userPermissionName."+_userPermissionName,_userPermissionName);
        DescriptionLabel.Text = Program.translator.getTranslated(
            "userPermissionDescription."+_userPermissionName,"");
        DescriptionLabel.Visible = DescriptionLabel.Text!="";
    }
    public const int ButtonsHeightInPixels = 40;
    public UserPermissionPromptForm(ScriptMethods methods)
    {
        Padding = new(0);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        TopMost = true;
        var fscale = UserPermissionPrompt.FormScale;
        var width = (int)(500*fscale);
        var height = (int)(300*fscale);
        Size = new Size(width,height);
        // UI
        mainTableLayout.Dock = DockStyle.Fill;
            // ScriptRequresLabel
        mainTableLayout.RowStyles.Add(new(SizeType.AutoSize));
        mainTableLayout.Controls.Add(ScriptRequresLabel,0,0);
        ScriptRequresLabel.Font = new(ScriptRequresLabel.Font.FontFamily,(float)(fscale)*10,FontStyle.Italic);
        Program.translator.bindToTranslateChanged(ScriptRequresLabel.Text,(txt)=>{
            ScriptRequresLabel.Text = txt.Replace("<name>",methods.DisplayName);
        });
            //PermNameLabel
        mainTableLayout.RowStyles.Add(new(SizeType.AutoSize));
        mainTableLayout.Controls.Add(PermNameLabel,0,1);
        PermNameLabel.Font = new(PermNameLabel.Font.FontFamily,(float)(fscale)*16,FontStyle.Bold);
            //DescriptionLabel
        mainTableLayout.RowStyles.Add(new(SizeType.Percent,1f));
        mainTableLayout.Controls.Add(DescriptionLabel,0,2);
        // Texts ClientSize
        mainTableLayout.ClientSizeChanged += (_,_)=>{
            var size = new Size(mainTableLayout.ClientSize.Width,0);
            // min
            PermNameLabel.MinimumSize = size;
            ScriptRequresLabel.MinimumSize = size;
            // max
            PermNameLabel.MaximumSize = size;
            ScriptRequresLabel.MaximumSize = size;
        }; Controls.Add(mainTableLayout);
        Program.translator.bindToTranslator(refreshPermissionLabels);
        //Buttons
        mainTableLayout.RowStyles.Add(new(SizeType.Absolute,(float)(fscale)*ButtonsHeightInPixels));
        mainTableLayout.Controls.Add(buttonsTableLayout,0,mainTableLayout.RowCount-1);
            //Decline
        buttonsTableLayout.ColumnStyles.Add(new(SizeType.Percent,1f));
        buttonsTableLayout.Controls.Add(DeclineButton,0,0);
        DeclineButton.Anchor = AnchorStyles.Left;
        Program.translator.bindToTranslateChanged(DeclineButton.Text,(txt)=>{
            DeclineButton.Text = txt;
        }); DeclineButton.Click += (_,_)=>{
            isChoiced = true;
            Decline?.Invoke(this,EventArgs.Empty);
            Close();
        };
            //Once
        buttonsTableLayout.ColumnStyles.Add(new(SizeType.Percent,1f));
        buttonsTableLayout.Controls.Add(OnceButton,1,0);
        OnceButton.Anchor = AnchorStyles.None;
        Program.translator.bindToTranslateChanged(OnceButton.Text,(txt)=>{
            OnceButton.Text = txt;
        }); OnceButton.Click += (_,_)=>{
            isChoiced = true;
            AcceptOnce?.Invoke(this,EventArgs.Empty);
            Close();
        };
            //Allow
        buttonsTableLayout.ColumnStyles.Add(new(SizeType.Percent,1f));
        buttonsTableLayout.Controls.Add(AllowButton,2,0);
        AllowButton.Anchor = AnchorStyles.Right;
        Program.translator.bindToTranslateChanged(AllowButton.Text,(txt)=>{
            AllowButton.Text = txt;
        });
        AllowButton.Click += (_,_)=>{
            isChoiced = true;
            Accept?.Invoke(this,EventArgs.Empty);
            Close();
        };
        // Винда кривая блин
        AllowButton.Margin = new(AllowButton.Margin.Left,AllowButton.Margin.Top,
            (int)(AllowButton.Margin.Left*2.5),AllowButton.Margin.Bottom);
        //ButtonFont
        var ButtonFontChanged = ()=>{
            var buttonHeight = buttonsTableLayout.ClientSize.Height;
            Font ButtonsFont = new(SystemFonts.DefaultFont.FontFamily,(buttonHeight-(
                buttonsTableLayout.Margin.Vertical+buttonsTableLayout.Padding.Vertical
            ))/2.25f);
            DeclineButton.Font = ButtonsFont;
            OnceButton.Font = ButtonsFont;
            AllowButton.Font = ButtonsFont;
        }; ButtonFontChanged();
        mainTableLayout.ClientSizeChanged += (_,_)=>ButtonFontChanged();
        //XD
        //Other
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
    public UserPermissionPromptForm form {get; private set;}
    public string UserPermissionName{
        get=>form._userPermissionName;
        set{
            form._userPermissionName=value;
            Program.mainWindow.Invoke(form.refreshPermissionLabels);
        }
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
            form.Text = Program.translator.getTranslated("PermissionRequestFrom")
                +" '"+_script.scriptName+"' [RoHelper]";
        });
    }
    public UserPermissionPrompt(LuaScript Script){
        _script = Script;
        form = new(Script.scriptMethods);
        Program.translator.bindToTranslator(RefreshFromText);
        Program.mainWindow.Invoke(()=>{
            form.Show(); SystemSounds.Exclamation.Play();
        });
    }
}