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

      var entries = (
        from entry in scripts
        let command = entry.Value as string
        where !string.IsNullOrWhiteSpace(command)
        select new { Name = entry.Key, Command = command }
      ).ToList();

      int maxNameLength = entries.Select(e => e.Name.Length).Max();

      listBox1.DataSource = entries.Select(
        e => new ScriptEntry {
          Name = e.Name,
          Command = e.Command,
          Text = e.Name.PadRight(maxNameLength + 2) + e.Command
        }
      ).ToList();

      Text = string.IsNullOrWhiteSpace(name) ? Path.GetFileName(Directory.GetCurrentDirectory()) : name;
    }

    private void GetScripts(out IDictionary<string, object> scripts, out string name) {
      scripts = null;
      name = null;
      const string fileName = "package.json";
      if (!File.Exists(fileName)) return;
      var pkg = ParseJson(fileName);
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

      bool useWsl = false;
      const string yarnIntegrityFileName = "node_modules\\.yarn-integrity";
      if (executable == "yarn" && File.Exists(yarnIntegrityFileName)) {
        var integrity = ParseJson(yarnIntegrityFileName);
        if (integrity.ContainsKey("systemParams") && Convert.ToString(integrity["systemParams"]).Contains("linux")) {
          useWsl = true;
        }
      }

      if (ModifierKeys.HasFlag(Keys.Control)) {
        command += useWsl ? "; read -p 'Press any key to continue . . .'" : " && pause";
      }

      // https://github.com/microsoft/vscode/issues/63156#issuecomment-484260724
      var p = useWsl
        ? Process.Start("bash", "-i -c \"bash -i -c '" + command.Replace("'", @"'\''") + "'\"")
        : Process.Start("cmd", "/c \"" + command + "\"");

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

    private IDictionary<string, object> ParseJson(string fileName) {
      var serializer = new JavaScriptSerializer();
      string content = File.ReadAllText(fileName);
      return serializer.DeserializeObject(content) as IDictionary<string, object>;
    }
  }
}
