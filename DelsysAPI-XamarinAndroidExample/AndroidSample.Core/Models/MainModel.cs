using AndroidSample.Core;
public class MainModel
{
    #region Singleton Pattern
    private MainModel()
    {
        mvc = 1; //default value for mvc, wouldnt change the data
    }
    public static MainModel Instance { get; } = new MainModel();
    #endregion
    // TODO add user details and sql connections
    private Delsys _del;
    public double mvc { get; set; }
    public Delsys del
    {
        get { return _del; }
        set { _del = value; }
    }
}