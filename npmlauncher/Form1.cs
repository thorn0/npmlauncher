using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace npmlauncher {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
      Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
      Populate();
    }

    private void Populate() {
      var serializer = new JavaScriptSerializer();
      string fileName = "package.json";
      if (!File.Exists(fileName)) Exit();
      var pkg = serializer.DeserializeObject(File.ReadAllText(fileName)) as IDictionary<string, object>;
      if (pkg == null || !pkg.ContainsKey("scripts")) Exit();
      var scripts = pkg["scripts"] as IDictionary<string, object>;
      if (scripts == null) Exit();
      listBox1.DataSource = (
        from entry in scripts
        let command = entry.Value as string
        where !string.IsNullOrWhiteSpace(command)
        select new ScriptEntry(entry.Key, command)
      ).ToList();
      string name = pkg.ContainsKey("name") ? pkg["name"] as string : null;
      if (pkg.ContainsKey("version") && !string.IsNullOrWhiteSpace(pkg["version"] as string)) {
        name = (string.IsNullOrWhiteSpace(name) ? "" : name + " ") + "v" + pkg["version"];
      }
      Text = (string.IsNullOrWhiteSpace(name) ? "" : name + ": ") + "npm run …";
    }

    private void listBox1_DoubleClick(object sender, EventArgs e) {
      var selected = (ScriptEntry)listBox1.SelectedItem;
      var command = "npm run " + selected.Name;
      if (!Directory.Exists("node_modules")) {
        command = "npm install && " + command;
      }
      Process.Start("cmd", "/c \"" + command + "\"");
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
