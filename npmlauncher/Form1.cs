using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace npmlauncher {
  public partial class Form1 : Form {
    [DllImport("user32.dll")]
    static extern int SetWindowText(IntPtr hWnd, string text);

    public Form1() {
      InitializeComponent();
      Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
      Populate();
    }

    private void Populate() {
      GetScripts(out IDictionary<string, object> scripts, out string name);
      if (scripts == null) Exit();
      listBox1.DataSource = (
        from entry in scripts
        let command = entry.Value as string
        where !string.IsNullOrWhiteSpace(command)
        select new ScriptEntry(entry.Key, command)
      ).ToList();
      Text = string.IsNullOrWhiteSpace(name) ? Path.GetFileName(Directory.GetCurrentDirectory()) : name;
    }

    private void GetScripts(out IDictionary<string, object> scripts, out string name) {
      scripts = null;
      name = null;
      const string fileName = "package.json";
      if (!File.Exists(fileName)) return;
      var serializer = new JavaScriptSerializer();
      var pkg = serializer.DeserializeObject(File.ReadAllText(fileName)) as IDictionary<string, object>;
      if (pkg == null || !pkg.ContainsKey("scripts")) return;
      scripts = pkg["scripts"] as IDictionary<string, object>;
      name = pkg.ContainsKey("name") ? pkg["name"] as string : null;
      if (pkg.ContainsKey("version") && !string.IsNullOrWhiteSpace(pkg["version"] as string)) {
        name = (string.IsNullOrWhiteSpace(name) ? "" : name + " ") + "v" + pkg["version"];
      }
    }

    private void listBox1_DoubleClick(object sender, EventArgs e) {
      string executable = File.Exists("yarn.lock") ? "yarn" : "npm";
      var selected = (ScriptEntry)listBox1.SelectedItem;
      var command = executable + " run " + selected.Name;
      if (!Directory.Exists("node_modules")) {
        command = executable + " install && " + command;
      }
      var p = Process.Start("cmd", "/c \"" + command + "\"");
      SpinWait.SpinUntil(() => p.MainWindowHandle != IntPtr.Zero);
      SetWindowText(p.MainWindowHandle, Text + ": " + selected.Name);
      Exit();
    }

    private void Exit() {
      if (Application.MessageLoop) {
        Application.Exit();
      } else {
        Environment.Exit(0);
      }
    }

    private void listBox1_KeyPress(object sender, KeyPressEventArgs e) {
      switch (e.KeyChar) {
        case (char)13:
          listBox1_DoubleClick(null, null);
          break;
        case (char)27:
          Exit();
          break;
      }
    }
  }
}
