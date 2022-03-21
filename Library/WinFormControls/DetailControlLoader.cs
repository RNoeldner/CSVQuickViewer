namespace CsvTools
{
  public class DetailControlLoader : TwoStepDataTableLoader
  {


    public DetailControlLoader(DetailControl detailControl) : base(
      actionSetDataTable: table => detailControl.DataTable = table,
      getDataTable: () => detailControl.DataTable,
      setRefreshDisplayAsync: detailControl.RefreshDisplayAsync,
      loadNextBatchAsync: func => detailControl.LoadNextBatchAsync = func,
      actionBegin: () => detailControl.DataMissing= false,
      actionFinished: wrapper => { detailControl.EndOfFile = () => wrapper?.EndOfFile ?? true; detailControl.DataMissing = !(wrapper?.EndOfFile ?? true); })
    {
    }
  }
}
