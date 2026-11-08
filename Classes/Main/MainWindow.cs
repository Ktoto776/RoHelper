using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;

namespace RoHelper;

class SettingsPanel
{
    private const int NameHeight = 33;
    private static Dictionary<Panel,SettingsPanel> parentSelected = new Dictionary<Panel,SettingsPanel>();
    public Panel settingsPanel;
    public SettingsPage settingsPage;
    Panel ParentPanel;
    private Label NameLabel;
    public string Name{
        get=>NameLabel.Text;
        set=>NameLabel.Text=value;
    }
    public bool Visible{
        get=>settingsPanel.Visible;
        set{
            settingsPanel.Visible=value;
            NameLabel.Visible=value;
        }
    }
    public SettingsPanel(Panel parent)
    {
        ParentPanel = parent;
        NameLabel = new Label{Dock=DockStyle.Top,
            Height=NameHeight,TextAlign=ContentAlignment.MiddleCenter};
        NameLabel.Font = new Font(NameLabel.Font.FontFamily,NameHeight/2.25f,FontStyle.Bold);
        parent.Controls.Add(NameLabel);
        settingsPanel = new Panel{Dock=DockStyle.Fill,Padding=new Padding(0,NameLabel.Height,0,0),
            Margin=new Padding(0)};
        parent.Controls.Add(settingsPanel);
        if (!parentSelected.ContainsKey(parent)){
            parentSelected[parent] = this;
        }else{
            Visible = false;
        }
        settingsPage = new SettingsPage();
        settingsPage.LayoutPanel.Dock = DockStyle.Fill;
        settingsPage.LayoutPanel.HorizontalScroll.Maximum = 0;
        settingsPage.LayoutPanel.HorizontalScroll.Visible = false;
        settingsPage.LayoutPanel.AutoScroll = true;
        settingsPanel.Controls.Add(settingsPage.LayoutPanel);
    }
    public void Activate(){
        parentSelected[ParentPanel].Visible = false;
        parentSelected[ParentPanel] = this;
        Visible = true;
    }
}

class MainWindow : Form
{
    const int leftPanelWidth = 150;
    const int leftButtonHeight = 50;
    TableLayoutPanel leftTable;
    FlowLayoutPanel leftSystemPanel;
    public FlowLayoutPanel leftAddonPanel;
    Button SettingsButtonGroup;
    private Panel SettingsParentPanel;
    public MainWindow()
    {
        Text = "RoHelper";
        var table = new TableLayoutPanel{ Dock = DockStyle.Fill,
            ColumnCount = 3,RowCount = 1
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, leftPanelWidth));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5f));

        leftTable = new TableLayoutPanel{ Dock = DockStyle.Fill,
            ColumnCount = 1,RowCount = 2,
        };
        leftSystemPanel = new FlowLayoutPanel{Margin=new Padding(),AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink};
        leftAddonPanel = new FlowLayoutPanel{Margin=new Padding(),Dock = DockStyle.Fill,
            AutoScroll = true,};

        leftTable.Controls.Add(leftSystemPanel, 0, 0);
        leftTable.Controls.Add(leftAddonPanel, 0, 1);

        table.Controls.Add(leftTable, 0, 0);
        
        var Splitter = new RoundedRect{Dock=DockStyle.Fill,MaxRoundCorner=true,
            Margin=new Padding()};
        table.Controls.Add(Splitter, 1, 0);
        SettingsParentPanel = new Panel { Dock = DockStyle.Fill }; //, AutoScroll = true
        table.Controls.Add(SettingsParentPanel, 2, 0);

        this.Controls.Add(table);

        SettingsButtonGroup = createLeftItemButton();
        leftSystemPanel.Controls.Add(SettingsButtonGroup);
        CreateMainSettings();
        
        leftSystemPanel.Controls.Add(createLeftItemButton());
        var buttonsSize = leftSystemPanel.ClientSize.Height;
        int borderHeight = Height - ClientSize.Height+table.Padding.Vertical+leftSystemPanel.Padding.Vertical;
        MinimumSize = new Size(leftPanelWidth+255,buttonsSize+borderHeight+5);
        using (Stream? iconStream = Assembly.GetExecutingAssembly().
            GetManifestResourceStream("RoHelper.resources.icon.ico")){
            if (iconStream != null)
            {
                Icon = new Icon(iconStream);
            }
        }
    }
    public Button createLeftItemButton()
    {
        var button = new Button();
        //button.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        button.Width = leftTable.ClientSize.Width-leftTable.Margin.Horizontal;
        button.Height = leftButtonHeight;
        //button.AutoSize = true;
        return button;
    }
    public SettingsPanel createSettingsPanel()
    {
        var panel = new SettingsPanel(SettingsParentPanel);
        return panel;
    }
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Save
        var data = Program.appData.getWindowData("MAIN");
        if (data!=null){
            Size = new Size(data.Width,data.Height);
            Location = new Point(data.X,data.Y);
        }
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        this.Hide();
        if (e.CloseReason == CloseReason.UserClosing){
            e.Cancel = true;
        }else{
            var data = new AppData_WindowSave();
            data.X = this.Location.X;
            data.Y = this.Location.Y;
            data.Width = this.Size.Width;
            data.Height = this.Size.Height;
            Program.appData.WindowData["MAIN"] = data;
        }
        base.OnFormClosing(e);
    }

    private void CreateMainSettings()
    {
        var MainSettingsG = createSettingsPanel();
        SettingsButtonGroup.Click += (_,_)=>{
            MainSettingsG.Activate();
        };
        Program.translator.bindToTranslateChanged("Settings",(string text) =>{
            SettingsButtonGroup.Text = text;
            MainSettingsG.Name = text;
        });
        var SettingsGroup = MainSettingsG.settingsPage.createGroup();
        SettingsGroup.HeaderText = "TEST";
    }
}