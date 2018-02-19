namespace npmlauncher {
  class ScriptEntry {
    public string Name { get; set; }
    public string Command { get; set; }
    public ScriptEntry(string key, string value) {
      Name = key;
      Command = value;
    }
    public override string ToString() {
      string separator = "\t\t";
      if (Name.Length >= 8) {
        separator = "\t";
      }
      return Name + separator + Command;
    }
  }
}
