namespace npmlauncher {
  class ScriptEntry {
    public string Name { get; set; }
    public string Command { get; set; }
    public string Text { get; set; }
    public override string ToString() {
      return Text;
    }
  }
}
