using System.Text.RegularExpressions;
using MoonSharp.Interpreter;

namespace RoHelper;

abstract class BaseSetting<T>
{
    public event EventHandler? ValueChanged;
    abstract public T value {get; set;}
    abstract public T defaultValue {get; set;}
    public const int PixelHeight = 33;
    public TableLayoutPanel tableLayout = new();
    public Label NameLabel = new();
    public string NameText{
        get=>NameLabel.Text;
        set=>NameLabel.Text=value;
    }
    public BaseSetting(SettingsPageGroup parent){
        tableLayout.Height = PixelHeight;
        tableLayout.RowCount = 1;
        tableLayout.ColumnCount = 2;
        parent.parentContainer.ClientSizeChanged += (_,_)=>{
            tableLayout.Width = parent.parentContainer.ClientSize.Width-parent.parentContainer.Padding.Horizontal;
        };
        parent.LayoutPanel.Controls.Add(tableLayout);
        tableLayout.Controls.Add(NameLabel,0,0);
        NameLabel.AutoSize = true;
        NameLabel.Height = PixelHeight;
        NameLabel.Font = new(NameLabel.Font.FontFamily,(float)(PixelHeight/2.15),FontStyle.Regular);
    }
    protected static void NumberKeyPress(TextBox tb, KeyPressEventArgs e, bool canBeNegative, bool withDot){
        if (char.IsDigit(e.KeyChar)||char.IsControl(e.KeyChar)) return;
        if (canBeNegative&&e.KeyChar=='-'&&(tb.SelectionStart==0&&!tb.Text.Contains('-'))) return;
        if (withDot&&(e.KeyChar=='.'||e.KeyChar==',')&&!(tb.Text.Contains('.')||tb.Text.Contains(','))) return;
        e.Handled = true;
    }
    private T? lastValue;
    protected void runChangedEvent(){
        var val = value;
        if (!EqualityComparer<T>.Default.Equals(lastValue, val))
            ValueChanged?.Invoke(this,EventArgs.Empty);
        lastValue = val;
    }
}

class IntSetting : BaseSetting<int>
{
    public TextBox textBox = new();
    override public int defaultValue{
        get{if (int.TryParse(textBox.PlaceholderText, out int result)) return result;
        else return 0;}
        set=>textBox.PlaceholderText=value.ToString();
    }
    public bool canBeNegative = false;
    override public int value{
        get{ if (int.TryParse(textBox.Text, out int result)) return result;
            else return defaultValue;}
        set{ textBox.Text=value.ToString();
            runChangedEvent();
        }
    }
    public IntSetting(SettingsPageGroup parent) : base(parent){
        textBox.KeyPress += (sender,e)=>
            NumberKeyPress((TextBox)(sender!),e,canBeNegative,false);
        textBox.KeyDown += (_,e)=>{
            if (e.KeyCode == Keys.Enter){
                runChangedEvent();
                e.SuppressKeyPress = true;
            }
        };
        tableLayout.Controls.Add(textBox,1,0);
        textBox.Dock = DockStyle.Fill;
        textBox.LostFocus += (_, _)=>runChangedEvent();
    }
}

class DoubleSetting : BaseSetting<double>
{
    public TextBox textBox = new();
    private double? parse(string text){
        if (double.TryParse(text.Replace(".",","), out double result)) return result;
        return null;
    }
    override public double defaultValue{
        get=>parse(textBox.PlaceholderText)??0;
        set=>textBox.PlaceholderText=value.ToString();
    }
    public bool canBeNegative = false;
    override public double value{
        get=>parse(textBox.Text)??defaultValue;
        set{ textBox.Text=value.ToString();
            runChangedEvent();
        }
    }
    public DoubleSetting(SettingsPageGroup parent) : base(parent){
        textBox.KeyPress += (sender,e)=>
            NumberKeyPress((TextBox)(sender!),e,canBeNegative,true);
        textBox.KeyDown += (_,e)=>{
            if (e.KeyCode == Keys.Enter){
                runChangedEvent();
                e.SuppressKeyPress = true;
            }
        };
        tableLayout.Controls.Add(textBox,1,0);
        textBox.Dock = DockStyle.Fill;
        textBox.LostFocus += (_, _)=>runChangedEvent();
    }
}

class FloatSetting : BaseSetting<float>
{
    public TextBox textBox = new();
    private float? parse(string text){
        if (float.TryParse(text.Replace(".",","), out float result)) return result;
        return null;
    }
    override public float defaultValue{
        get=>parse(textBox.PlaceholderText)??0;
        set=>textBox.PlaceholderText=value.ToString();
    }
    public bool canBeNegative = false;
    override public float value{
        get=>parse(textBox.Text)??defaultValue;
        set{ textBox.Text=value.ToString();
            runChangedEvent();
        }
    }
    public FloatSetting(SettingsPageGroup parent) : base(parent){
        textBox.KeyPress += (sender,e)=>
            NumberKeyPress((TextBox)(sender!),e,canBeNegative,true);
        textBox.KeyDown += (_,e)=>{
            if (e.KeyCode == Keys.Enter){
                runChangedEvent();
                e.SuppressKeyPress = true;
            }
        };
        tableLayout.Controls.Add(textBox,1,0);
        textBox.Dock = DockStyle.Fill;
        textBox.LostFocus += (_, _)=>runChangedEvent();
    }
}

class StringSetting : BaseSetting<string>
{
    public TextBox textBox = new();
    override public string defaultValue{
        get=>textBox.PlaceholderText;
        set=>textBox.PlaceholderText=value;
    }
    override public string value{
        get=>textBox.Text;
        set{ textBox.Text=value;
            runChangedEvent();
        }
    }
    public StringSetting(SettingsPageGroup parent) : base(parent){
        textBox.KeyDown += (_,e)=>{
            if (e.KeyCode == Keys.Enter){
                runChangedEvent();
                e.SuppressKeyPress = true;
            }
        };
        tableLayout.Controls.Add(textBox,1,0);
        textBox.Dock = DockStyle.Fill;
        textBox.LostFocus += (_, _)=>runChangedEvent();
    }
}

class SettingsPageGroup{
    public Panel parentContainer;
    public FlowLayoutPanel LayoutPanel = new FlowLayoutPanel{
        Margin=new Padding(),
        Padding=new Padding(),
        AutoSize=true,
    };
    private Label headerLabel = new Label{TextAlign=ContentAlignment.MiddleCenter,
        Margin=new Padding()};
    public float HeaderFontSize{
        get=>headerLabel.Font.Size;
        set{
            var oldFont = headerLabel.Font;
            var font = new Font(oldFont.FontFamily,value,FontStyle.Bold);
            headerLabel.Font = font;
            headerLabel.Height = (int)(value*2.25f);
        }
    }
    const float DefaultHeaderFontSize = 15;
    public string HeaderText{
        get=>headerLabel.Text;
        set{
            headerLabel.Visible = value!="";
            headerLabel.Text = value;
        }
    }
    public SettingsPageGroup(Panel parentContainer){
        this.parentContainer = parentContainer;
        LayoutPanel.Controls.Add(headerLabel);
        LayoutPanel.ClientSizeChanged += (a,b)=>
            headerLabel.Width=LayoutPanel.ClientSize.Width;
        parentContainer.Controls.Add(LayoutPanel);
        LayoutPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        HeaderFontSize = DefaultHeaderFontSize;
        HeaderText = "";
    }
    public IntSetting createIntSetting(){
        return new(this);
    }
    public DoubleSetting createDoubleSetting(){
        return new(this);
    }
    public DoubleSetting createFloatSetting(){
        return new(this);
    }
    public StringSetting createStringSetting(){
        return new(this);
    }
}

class SettingsPage
{
    private List<SettingsPageGroup> Groups = [];
    public FlowLayoutPanel LayoutPanel = new FlowLayoutPanel{Margin=new Padding()};
    public List<SettingsPageGroup> getGroups(){
        List<SettingsPageGroup> groups = [];
        foreach (SettingsPageGroup group in Groups) groups.Add(group);
        return groups;
    }
    public SettingsPageGroup createGroup(){
        var group = new SettingsPageGroup(LayoutPanel);
        var groupPanel = group.LayoutPanel;
        groupPanel.Anchor = AnchorStyles.None;
        LayoutPanel.ClientSizeChanged += (_,_)=>groupPanel.Width=LayoutPanel.ClientSize.Width;
        return group;
    }
}