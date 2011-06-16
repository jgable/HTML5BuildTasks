using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.Collections;

namespace FrogMSBuild
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
    /// Combine javascript file
    /// </summary>
    Javascript
  }

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

    private CombinationMode mode = FrogMSBuild.CombinationMode.TextLine;
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
    /// <returns></returns>
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
          case FrogMSBuild.CombinationMode.Binary:
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
            bool newLine = this.mode != FrogMSBuild.CombinationMode.Text;
            bool isJavascript = this.mode == FrogMSBuild.CombinationMode.Javascript;
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
