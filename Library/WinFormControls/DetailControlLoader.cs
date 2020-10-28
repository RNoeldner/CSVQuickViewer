namespace CsvTools
{
  public class DetailControlLoader : TwoStepDataTableLoader
  {
    public DetailControlLoader(DetailControl detailControl) : base(
      table => detailControl.DataTable = table,
      () => detailControl.DataTable, detailControl.RefreshDisplayAsync,
      func => detailControl.LoadNextBatchAsync = func,
      () => detailControl.toolStripButtonNext.Enabled = false,
      wrapper =>
      {
        detailControl.EndOfFile = () =>
          wrapper?.EndOfFile ?? true;

        detailControl.toolStripButtonNext.Visible = wrapper != null;
        detailControl.toolStripButtonNext.Enabled = wrapper != null;
      })
    {
    }
  }
}