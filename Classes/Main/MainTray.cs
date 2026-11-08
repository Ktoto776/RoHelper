namespace RoHelper;

using System.Reflection;
using System.IO;
using System;

public class MainTray
{
    private NotifyIcon trayIcon;
    private ContextMenuStrip trayMenu;
    public ToolStripItem OpenWindowButton; 
    public ToolStripItem ExitProgramButton; 
    public MainTray(){
        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add(new ToolStripSeparator());
        OpenWindowButton = trayMenu.Items.Add("Open", null, (_,_)=>OpenWindow());
        ExitProgramButton = trayMenu.Items.Add("Exit", null, ExitButton);

        // trayMenu.Items.Insert(0,new ToolStripComboBox());

        Program.translator.bindToTranslateChanged("ExitApp", (string text) =>
            ExitProgramButton.Text = text);
        Program.translator.bindToTranslateChanged("OpenWindow", (string text) =>
            OpenWindowButton.Text = text);

        trayIcon = new NotifyIcon();
        trayIcon.Text = "RoHelper";
        trayIcon.ContextMenuStrip = trayMenu;
        trayIcon.Visible = true;
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream? stream = assembly.GetManifestResourceStream("RoHelper.resources.icon.ico"))
        {
            if (stream != null)
            {
                trayIcon.Icon = new Icon(stream);
            }
        }
        Random random = new Random();
        if (random.Next(50)==0){ // Шанс 1 к 50
            Console.WriteLine("std::cout << Easter egg in tray name << std::endl");
            trayIcon.Text = "JUST rohelper XD";
        }

        // Обработка двойного клика для восстановления окна
        trayIcon.DoubleClick += (_,_) => OpenWindow();
    }
    private void ExitButton(object? sender, EventArgs e){
        Application.Exit();
    }
    private void OpenWindow(){
        Program.mainWindow.Show();
        Program.mainWindow.Focus();
    }
}