using System;
using System.Collections.Generic;


namespace CsvTools
{
  public class Sheet
  {
    public readonly string Name;
    public readonly bool Hidden;
    
    public Sheet(string name, bool hidden)
    {
      if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Name of sheet can not be empty", nameof(name));
      Name = name;
      Hidden = hidden;
    }

    public override bool Equals(object obj) => obj is Sheet sheets && Name == sheets.Name && Hidden == sheets.Hidden;

    public override int GetHashCode()
    {
      var hashCode = 1548707919;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
      hashCode = hashCode * -1521134295 + Hidden.GetHashCode();
      return hashCode;
    }

    public override string ToString() => (Hidden ? "(" : string.Empty) + Name + (Hidden ? ")*" : string.Empty);
  }
}
