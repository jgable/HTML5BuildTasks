using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace HTML5BuildTasks
{
  public enum CombinationMode
  {
    /// <summary>
    /// Combine as binary
    /// </summary>
    Binary,
    /// <summary>
    /// Combine as text
    /// </summary>
    Text,
    /// <summary>
    /// Combine as text. The next file will appear in a new line.
    /// </summary>
    TextLine,
    /// <summary>
    /// Combine javascript file.  The next file is added after a semi-colon and a new line.
    /// </summary>
    Javascript
  }

  /// <summary>
  /// Combines a set of files as Binary, Text, TextLine, or Javascript.
  /// </summary>
  public class FileCombinator : Task
  {

    /// <summary>
    /// Gets or sets source files which will be combined.
    /// </summary>
    [Required]
    public ITaskItem[] SourceFiles { get; set; }

    /// <summary>
    /// Gets or sets value indicates whether we delete source files after finish combinating.
    /// </summary>
    public bool RemoveSourceFiles { get; set; }

    private CombinationMode mode = HTML5BuildTasks.CombinationMode.TextLine;
    /// <summary>
    /// Gets or sets the combination mode.
    /// </summary>
    /// <value>The combination mode.</value>
    public string CombinationMode
    {
      get { return this.mode.ToString(); }
      set 
      {
        this.mode = (CombinationMode)Enum.Parse(typeof(CombinationMode), value);
      }
    }

    /// <summary>
    /// Gets or sets the target file.
    /// </summary>
    /// <value>The target file.</value>
    [Required]
    public ITaskItem TargetFile
    {
      get;
      set;
    }

    /// <summary>
    /// Executes this instance.
    /// </summary>
    /// <returns>true if success; otherwise, false.</returns>
    public override bool Execute()
    {
      if (string.IsNullOrEmpty(this.TargetFile.ItemSpec))
      {
        base.Log.LogError("Frog File Combinator: You must supply a value for TargetFile");
        return false;
      }

      if (this.SourceFiles != null && this.SourceFiles.Length > 0)
      {
        //Separates source files by folder
        Hashtable hashSourceFiles = new Hashtable();
        foreach (ITaskItem item in this.SourceFiles)
        {
          string folder = Path.GetFullPath(item.ItemSpec).ToLowerInvariant();
          object hashItem = hashSourceFiles[folder];
          if (hashItem == null)
          {
            hashItem = new List<string>();
            hashSourceFiles.Add(folder, hashItem);
          }

          List<string> list = (List<string>)hashItem;
          list.Add(item.ItemSpec);
        }

        switch (this.mode)
        {
          case HTML5BuildTasks.CombinationMode.Binary:
            using (FileStream fs = new FileStream(this.TargetFile.ItemSpec, FileMode.Create, FileAccess.Write, FileShare.None, 0x8000))
            {
              byte[] buffer = new byte[0x8000];

              foreach (ITaskItem item in this.SourceFiles)
              {
                using (FileStream sourceStream = File.OpenRead(item.ItemSpec))
                {
                  for (long i = sourceStream.Length; i > 0; i -= 0x8000)
                  {
                    int numRead = sourceStream.Read(buffer, 0, 0x8000);
                    fs.Write(buffer, 0, numRead);
                  }
                }
                if (this.RemoveSourceFiles)
                  File.Delete(item.ItemSpec);
              }
            }
            break;
          default:            //case CombinationMode.Text:            //case CombinationMode.TextLine:
            bool newLine = this.mode != HTML5BuildTasks.CombinationMode.Text;
            bool isJavascript = this.mode == HTML5BuildTasks.CombinationMode.Javascript;
            using (StreamWriter sw = File.CreateText(this.TargetFile.ItemSpec))
            {
              foreach (ITaskItem item in this.SourceFiles)
              {
                string s = File.ReadAllText(item.ItemSpec);
                if (newLine)
                {
                  if (isJavascript && s.EndsWith(";"))
                    sw.Write(s);
                  else
                    sw.WriteLine(s);
                }
                else
                  sw.Write(s);
                if (this.RemoveSourceFiles)
                  File.Delete(item.ItemSpec);
              }
            }
            break;
        }
      }

      return true;
    }
  }
}
