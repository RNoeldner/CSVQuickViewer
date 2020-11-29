namespace CsvTools
{
  public class DetailControlLoader : TwoStepDataTableLoader
  {
    public DetailControlLoader(DetailControl detailControl) : base(
      table => detailControl.DataTable = table,
      () => detailControl.DataTable, detailControl.RefreshDisplayAsync,
      func => detailControl.LoadNextBatchAsync = func,
      () => detailControl.ToolStripButtonNext.Enabled = false,
      wrapper =>
      {
        detailControl.EndOfFile = () =>
          wrapper?.EndOfFile ?? true;
        detailControl.SafeBeginInvoke(() =>
        {
          detailControl.ToolStripButtonNext.Visible = wrapper != null;
          detailControl.ToolStripButtonNext.Enabled = wrapper != null;
        });
      })
    {
    }
  }
}