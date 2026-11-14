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
    public LuaScript.LuaEvent Destroying;
    public void Destroy(){
        form.Close();
        Destroying.trigger();
    }
    public ScriptWindow(ScriptMethods methods){
        this.methods = methods;
        Destroying = new(methods.script);
        form = new(this);
        if (Program.mainWindow.InvokeRequired)
        {
            Program.mainWindow.Invoke(new Action(() => form.Show()));
            return;
        }
    }
}