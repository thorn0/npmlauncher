using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace npmlauncher
{
  class ScriptEntry
  {
    public string Name { get; set; }
    public string Command { get; set; }
    public ScriptEntry(string key, string value)
    {
      Name = key;
      Command = value;
    }
    public override string ToString()
    {
      return Name + "\t" + Command;
    }
  }
}
