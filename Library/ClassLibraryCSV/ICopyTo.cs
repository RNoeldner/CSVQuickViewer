namespace CsvTools
{
  public interface ICopyTo<in T>
  {
    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    void CopyTo(T other);
  }
}