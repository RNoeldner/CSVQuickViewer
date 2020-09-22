using System;

namespace FastColoredTextBoxNS
{
  /// <summary>
  /// Line index and char index
  /// </summary>
  public struct Place : IEquatable<Place>
  {
    public int iChar;
    public int iLine;

    public Place(int iChar, int iLine)
    {
      this.iChar = iChar;
      this.iLine = iLine;
    }

    public void Offset(int dx, int dy)
    {
      iChar += dx;
      iLine += dy;
    }

    public bool Equals(Place other) => iChar == other.iChar && iLine == other.iLine;

    public override bool Equals(object obj) => (obj is Place) && Equals((Place)obj);

    public override int GetHashCode() => iChar.GetHashCode() ^ iLine.GetHashCode();

    public static bool operator !=(Place p1, Place p2) => !p1.Equals(p2);

    public static bool operator ==(Place p1, Place p2) => p1.Equals(p2);

    public static bool operator <(Place p1, Place p2)
    {
      if (p1.iLine < p2.iLine) return true;
      if (p1.iLine > p2.iLine) return false;
      if (p1.iChar < p2.iChar) return true;
      return false;
    }

    public static bool operator <=(Place p1, Place p2)
    {
      if (p1.Equals(p2)) return true;
      if (p1.iLine < p2.iLine) return true;
      if (p1.iLine > p2.iLine) return false;
      if (p1.iChar < p2.iChar) return true;
      return false;
    }

    public static bool operator >(Place p1, Place p2)
    {
      if (p1.iLine > p2.iLine) return true;
      if (p1.iLine < p2.iLine) return false;
      if (p1.iChar > p2.iChar) return true;
      return false;
    }

    public static bool operator >=(Place p1, Place p2)
    {
      if (p1.Equals(p2)) return true;
      if (p1.iLine > p2.iLine) return true;
      if (p1.iLine < p2.iLine) return false;
      if (p1.iChar > p2.iChar) return true;
      return false;
    }

    public static Place operator +(Place p1, Place p2) => new Place(p1.iChar + p2.iChar, p1.iLine + p2.iLine);

    public static Place Empty => new Place();

    public override string ToString() => "(" + iChar + "," + iLine + ")";
  }
}
