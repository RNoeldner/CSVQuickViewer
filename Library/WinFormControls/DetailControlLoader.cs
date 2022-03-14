namespace CsvTools
{
  public class DetailControlLoader : TwoStepDataTableLoader
  {
    public DetailControlLoader(DetailControl detailControl) : base(
      table => detailControl.DataTable = table,
      () => detailControl.DataTable, detailControl.RefreshDisplayAsync,
      func => detailControl.LoadNextBatchAsync = func,
      () => detailControl.DataMissing= false,
      wrapper =>
      {
        detailControl.EndOfFile = () =>
          wrapper?.EndOfFile ?? true;
        detailControl.DataMissing = (wrapper != null);
      })
    {
    }
  }
}
