using System.Text.RegularExpressions;

namespace RoHelper;

class SettingsPageGroup{
    private Panel parentContainer;
    public FlowLayoutPanel LayoutPanel = new FlowLayoutPanel{
        Margin=new Padding(),
        Padding=new Padding(),
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