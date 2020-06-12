using System;
using JetBrains.Annotations;

namespace CsvTools
{
  public class ValueFormatReadOnly : IValueFormat, IEquatable<IValueFormat>
  {
    public ValueFormatReadOnly(DataType dataType, string dateFormat, string dateSeparator, char decimalSeparatorChar,
      string displayNullAs, [NotNull] string asFalse, char groupSeparatorChar, string numberFormat,
      string timeSeparator,
      [NotNull] string asTrue)
    {
      DataType = dataType;
      DateFormat = dateFormat;
      DateSeparator = dateSeparator;
      DecimalSeparatorChar = decimalSeparatorChar;
      DisplayNullAs = displayNullAs;
      False = asFalse;
      GroupSeparatorChar = groupSeparatorChar;
      NumberFormat = numberFormat;
      TimeSeparator = timeSeparator;
      True = asTrue;
    }

    public bool Equals(IValueFormat other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return DataType == other.DataType && DateFormat == other.DateFormat && DateSeparator == other.DateSeparator &&
             DecimalSeparatorChar == other.DecimalSeparatorChar && DisplayNullAs == other.DisplayNullAs &&
             False == other.False && GroupSeparatorChar == other.GroupSeparatorChar &&
             NumberFormat == other.NumberFormat && TimeSeparator == other.TimeSeparator && True == other.True;
    }

    public DataType DataType { get; }
    public string DateFormat { get; }
    public string DateSeparator { get; }
    public char DecimalSeparatorChar { get; }
    public string DisplayNullAs { get; }
    public string False { get; }
    public char GroupSeparatorChar { get; }
    public string NumberFormat { get; }
    public string TimeSeparator { get; }
    public string True { get; }

    public static IValueFormat ReadOnly(IValueFormat rw) => new ValueFormatReadOnly(rw.DataType,
      rw.DateFormat, rw.DateSeparator, rw.DecimalSeparatorChar, rw.DisplayNullAs, rw.False, rw.GroupSeparatorChar,
      rw.NumberFormat, rw.TimeSeparator, rw.True
    );

    public IValueFormat ReadWrite() => new ValueFormat(DataType)
    {
      DateFormat = DateFormat,
      DateSeparator = DateSeparator,
      DecimalSeparator = DecimalSeparatorChar.ToString(),
      DisplayNullAs = DisplayNullAs,
      False = False,
      GroupSeparator = GroupSeparatorChar.ToString(),
      NumberFormat = NumberFormat,
      TimeSeparator = TimeSeparator,
      True = True
    };

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((ValueFormatReadOnly) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int) DataType;
        hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(DisplayNullAs);
        switch (DataType)
        {
          case DataType.Integer:
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(NumberFormat);
            break;

          case DataType.Numeric:
          case DataType.Double:
            hashCode = (hashCode * 397) ^ GroupSeparatorChar.GetHashCode();
            hashCode = (hashCode * 397) ^ DecimalSeparatorChar.GetHashCode();
            hashCode = (hashCode * 397) ^ NumberFormat.GetHashCode();
            break;

          case DataType.DateTime:
            hashCode = (hashCode * 397) ^ DateFormat.GetHashCode();
            hashCode = (hashCode * 397) ^ DateSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ TimeSeparator.GetHashCode();
            break;

          case DataType.Boolean:
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(False);
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(True);
            break;

          default:
            hashCode = (hashCode * 397) ^ DateFormat.GetHashCode();
            hashCode = (hashCode * 397) ^ DateSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ DecimalSeparatorChar.GetHashCode();
            hashCode = (hashCode * 397) ^ DisplayNullAs.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(False);
            hashCode = (hashCode * 397) ^ GroupSeparatorChar.GetHashCode();
            hashCode = (hashCode * 397) ^ NumberFormat.GetHashCode();
            hashCode = (hashCode * 397) ^ TimeSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(True);
            break;
        }

        return hashCode;
      }
    }
  }
}