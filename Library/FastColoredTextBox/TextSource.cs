using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace FastColoredTextBoxNS
{
  /// <summary>
  /// This class contains the source text (chars and styles).
  /// It stores a text lines, the manager of commands, undo/redo stack, styles.
  /// </summary>
  public class TextSource : IList<Line>, IDisposable
  {
    protected readonly List<Line> lines = new List<Line>();
    protected LinesAccessor linesAccessor;
    private int lastLineUniqueId;
    public CommandManager Manager { get; set; }

    private FastColoredTextBox currentTB;
    /// <summary>
    /// Styles
    /// </summary>
    public readonly Style[] Styles;
    /// <summary>
    /// Occurs when line was inserted/added
    /// </summary>
    public event EventHandler<LineInsertedEventArgs> LineInserted;
    /// <summary>
    /// Occurs when line was removed
    /// </summary>
    public event EventHandler<LineRemovedEventArgs> LineRemoved;
    /// <summary>
    /// Occurs when text was changed
    /// </summary>
    public event EventHandler<TextChangedEventArgs> TextChanged;
    /// <summary>
    /// Occurs when recalc is needed
    /// </summary>
    public event EventHandler<TextChangedEventArgs> RecalcNeeded;
    /// <summary>
    /// Occurs when recalc wordwrap is needed
    /// </summary>
    public event EventHandler<TextChangedEventArgs> RecalcWordWrap;
    /// <summary>
    /// Occurs before text changing
    /// </summary>
    public event EventHandler<TextChangingEventArgs> TextChanging;
    /// <summary>
    /// Occurs after CurrentTB was changed
    /// </summary>
    public event EventHandler CurrentTBChanged;
    /// <summary>
    /// Current focused FastColoredTextBox
    /// </summary>
    public FastColoredTextBox CurrentTB
    {
      get => currentTB;
      set
      {
        if (currentTB == value)
          return;
        currentTB = value;
        OnCurrentTBChanged();
      }
    }

    public virtual void ClearIsChanged()
    {
      foreach (var line in lines)
        line.IsChanged = false;
    }

    public virtual Line CreateLine() => new Line(GenerateUniqueLineId());

    private void OnCurrentTBChanged()
    {
      if (CurrentTBChanged != null)
        CurrentTBChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Default text style
    /// This style is using when no one other TextStyle is not defined in Char.style
    /// </summary>
    public TextStyle DefaultStyle { get; set; }

    public TextSource(FastColoredTextBox currentTB)
    {
      CurrentTB = currentTB;
      linesAccessor = new LinesAccessor(this);
      Manager = new CommandManager(this);

      if (Enum.GetUnderlyingType(typeof(StyleIndex)) == typeof(uint))
        Styles = new Style[32];
      else
        Styles = new Style[16];

      InitDefaultStyle();
    }

    public virtual void InitDefaultStyle() => DefaultStyle = new TextStyle(null, null, FontStyle.Regular);

    public virtual Line this[int i]
        {
            get => lines[i];
            set => throw new NotImplementedException();
        }

    public virtual bool IsLineLoaded(int iLine) => lines[iLine] != null;

    /// <summary>
    /// Text lines
    /// </summary>
    public virtual IList<string> GetLines() => linesAccessor;

    public IEnumerator<Line> GetEnumerator() => lines.GetEnumerator();

    IEnumerator System.Collections.IEnumerable.GetEnumerator() => (lines as IEnumerator);

    public virtual int BinarySearch(Line item, IComparer<Line> comparer) => lines.BinarySearch(item, comparer);

    public virtual int GenerateUniqueLineId() => lastLineUniqueId++;

    public virtual void InsertLine(int index, Line line)
    {
      lines.Insert(index, line);
      OnLineInserted(index);
    }

    public virtual void OnLineInserted(int index) => OnLineInserted(index, 1);

    public virtual void OnLineInserted(int index, int count)
    {
      if (LineInserted != null)
        LineInserted(this, new LineInsertedEventArgs(index, count));
    }

    public virtual void RemoveLine(int index) => RemoveLine(index, 1);

    public virtual bool IsNeedBuildRemovedLineIds => LineRemoved != null;

    public virtual void RemoveLine(int index, int count)
    {
      var removedLineIds = new List<int>();
      //
      if (count > 0)
        if (IsNeedBuildRemovedLineIds)
          for (var i = 0; i < count; i++)
            removedLineIds.Add(this[index + i].UniqueId);
      //
      lines.RemoveRange(index, count);

      OnLineRemoved(index, count, removedLineIds);
    }

    public virtual void OnLineRemoved(int index, int count, List<int> removedLineIds)
    {
      if (count > 0)
        if (LineRemoved != null)
          LineRemoved(this, new LineRemovedEventArgs(index, count, removedLineIds));
    }

    public virtual void OnTextChanged(int fromLine, int toLine)
    {
      if (TextChanged != null)
        TextChanged(this, new TextChangedEventArgs(Math.Min(fromLine, toLine), Math.Max(fromLine, toLine)));
    }

    public class TextChangedEventArgs : EventArgs
    {
      public int iFromLine;
      public int iToLine;

      public TextChangedEventArgs(int iFromLine, int iToLine)
      {
        this.iFromLine = iFromLine;
        this.iToLine = iToLine;
      }
    }

    public virtual int IndexOf(Line item) => lines.IndexOf(item);

    public virtual void Insert(int index, Line item) => InsertLine(index, item);

    public virtual void RemoveAt(int index) => RemoveLine(index);

    public virtual void Add(Line item) => InsertLine(Count, item);

    public virtual void Clear() => RemoveLine(0, Count);

    public virtual bool Contains(Line item) => lines.Contains(item);

    public virtual void CopyTo(Line[] array, int arrayIndex) => lines.CopyTo(array, arrayIndex);

    /// <summary>
    /// Lines count
    /// </summary>
    public virtual int Count => lines.Count;

    public virtual bool IsReadOnly => false;

    public virtual bool Remove(Line item)
    {
      var i = IndexOf(item);
      if (i >= 0)
      {
        RemoveLine(i);
        return true;
      }
      else
        return false;
    }

    public virtual void NeedRecalc(TextChangedEventArgs args)
    {
      if (RecalcNeeded != null)
        RecalcNeeded(this, args);
    }

    public virtual void OnRecalcWordWrap(TextChangedEventArgs args)
    {
      if (RecalcWordWrap != null)
        RecalcWordWrap(this, args);
    }

    public virtual void OnTextChanging()
    {
      string temp = null;
      OnTextChanging(ref temp);
    }

    public virtual void OnTextChanging(ref string text)
    {
      if (TextChanging != null)
      {
        var args = new TextChangingEventArgs() { InsertingText = text };
        TextChanging(this, args);
        text = args.InsertingText;
        if (args.Cancel)
          text = string.Empty;
      };
    }

    public virtual int GetLineLength(int i) => lines[i].Count;

    public virtual bool LineHasFoldingStartMarker(int iLine) => !string.IsNullOrEmpty(lines[iLine].FoldingStartMarker);

    public virtual bool LineHasFoldingEndMarker(int iLine) => !string.IsNullOrEmpty(lines[iLine].FoldingEndMarker);

    public virtual void Dispose()
    {
      ;
    }

    public virtual void SaveToFile(string fileName, Encoding enc)
    {
      using (var sw = new StreamWriter(fileName, false, enc))
      {
        for (var i = 0; i < Count - 1; i++)
          sw.WriteLine(lines[i].Text);

        sw.Write(lines[Count - 1].Text);
      }
    }
  }
}
