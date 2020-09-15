// /*
// * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com *
// * This program is free software: you can redistribute it and/or modify it under the terms of the
// GNU Lesser Public
// * License as published by the Free Software Foundation, either version 3 of the License, or (at
// your option) any later version. *
// * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty
// * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for
// more details. *
// * You should have received a copy of the GNU Lesser Public License along with this program.
// * If not, see http://www.gnu.org/licenses/ . *
// */

using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace CsvTools
{
  public class SyntaxHighlighterOData : SyntaxHighlighterBase
  {
    private readonly Regex m_ODataExpressionsRegex = new Regex(
      @"\b(gt|eq|and|or|lt|ge|le|ne|endswith|startswith|substringof|indexof|replace|substring|tolower|toupper|trim|concat|round|floor|ceiling|length)\b",
      RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly Regex m_ODataNumberRegex = new Regex(@"\b(\d+[\.]?\d*|true|false|null)\b",
      RegexOptions.Singleline | RegexCompiledOption);

    private readonly Regex m_ODataStringRegex =
      new Regex(@"'([^\\']|\\')*'", RegexOptions.Singleline | RegexCompiledOption);

    private readonly Regex m_ODataTypesRegex = new Regex(
      @"\b(Edm.String|Edm.Int16|Edm.Int32|Edm.Int64|Edm.DateTimeOffset|Edm.Date|Edm.Single|Edm.Double|Edm.Decimal|Edm.Boolean|Edm.Guid)\b",
      RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SyntaxHighlighterOData(FastColoredTextBox currentTb) : base(currentTb)
    {
      currentTb.TextChangedDelayed += (sender, e) => Highlight(e.ChangedRange);
    }

    public override void Highlight(Range range)
    {
      range.ClearStyle(RedStyle, MagentaStyle, BrownStyle, BlueStyle);

      // string highlighting
      range.SetStyle(RedStyle, m_ODataStringRegex);

      // number highlighting
      range.SetStyle(MagentaStyle, m_ODataNumberRegex);
      // types highlighting
      range.SetStyle(BrownStyle, m_ODataTypesRegex);

      // Expressions
      range.SetStyle(BlueStyle, m_ODataExpressionsRegex);
    }
  }
}