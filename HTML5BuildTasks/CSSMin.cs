using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HTML5BuildTasks
{
    /// <summary>
    /// A really, really basic regex based CSS Minifier.
    /// </summary>
    public class CSSMin
    {
        public string MinifyCSS(string css)
        {
            Regex.Replace(css, "/\\*.+?\\*/", "", RegexOptions.Singleline);
            css = css.Replace("  ", string.Empty);
            css = css.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, string.Empty);
            css = css.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            css = css.Replace(Environment.NewLine, string.Empty);
            css = css.Replace("\\t", string.Empty);
            css = css.Replace(" {", "{");
            css = css.Replace(" :", ":");
            css = css.Replace(": ", ":");
            css = css.Replace(", ", ",");
            css = css.Replace("; ", ";");
            css = css.Replace(";}", "}");
            css = Regex.Replace(css, "/\\*[^\\*]*\\*+([^/\\*]*\\*+)*/", "$1");
            css = Regex.Replace(css, "(?<=[>])\\s{2,}(?=[<])|(?<=[>])\\s{2,}(?=&nbsp;)|(?<=&ndsp;)\\s{2,}(?=[<])", string.Empty);
    
            return css;
        }
    }
}
