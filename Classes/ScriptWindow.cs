using MoonSharp.Interpreter;

namespace RoHelper;

class ScriptWindowForm : Form
{
    private ScriptWindow scriptWindow;
    public ScriptWindowForm(ScriptWindow scriptWindow){ 
        this.scriptWindow = scriptWindow;
    }
}

class ScriptWindow
{
    [MoonSharpHidden]
    public ScriptMethods methods;
    private ScriptWindowForm form;
    public bool isOpened;
    public void Destroy(){
        form.Close();
    }
    public ScriptWindow(ScriptMethods methods){
        this.methods = methods;
        form = new(this);
        if (Program.mainWindow.InvokeRequired)
        {
            Program.mainWindow.Invoke(new Action(() => form.Show()));
            return;
        }
    }
}