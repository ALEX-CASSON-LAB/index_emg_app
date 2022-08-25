using AndroidSample.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class MainModel
{
    #region Singleton Pattern
    private MainModel()
    {
        mvc = 1; //default value for mvc, wouldnt change the data
    }
    public static MainModel Instance { get; } = new MainModel();
    #endregion

    private Delsys _del;
    private double[][] _data;
    public double mvc { get; set; }

    private IndexDatabase _database;

    public Session currentSession;
    public bool realTimeCollection;

    public List<Exercise> availableExercises;

    private List<List<double>> exerciseMeans = new List<List<double>>(); 

    public Delsys del
    {
        get { return _del; }
        set { _del = value; }
    }
    public double[][] data
    {
        get { return _data; }
        set { _data = value; }
    }

    public async void setupDatabase()
    {
        _database = await IndexDatabase.Instance;
        //await _database.ClearSessionTableAsync();
    }

    #region Session methods
    public void startSession()
    {
        currentSession = new Session();
        currentSession.date = System.DateTime.Now.ToLocalTime();
        SetUpMvcs();
        _database.SaveItemAsync(currentSession).Wait();
    }
    public async void RecordCurrentSession()
    {
        await _database.SaveItemAsync(currentSession);
    }

    public List<Session> GetAllSessions()
    {
        List<Session> allSessions = _database.GetItemsAsync().Result;
        return allSessions;
    }

    public Session GetSession(int id)
    {
        Session s = _database.GetSessionAsync(id).Result;
        return s;
    }

    public void EndSession()
    {
        if (del != null)
        {
            if(del.BTPipeline != null)
            {
                del.PipelineDisarm();
                del.BTPipeline = null;
            }
        }
    }
    #endregion

    #region Exercise methods
    public Exercise GetExercise(int id)
    {
        foreach (Exercise e in availableExercises)
        {
            if (e.Id == id)
            {
                return e;
            }
        }
        return null; //todo ERROR
    }
    public Exercise GetExercise(string id)
    {
        foreach (Exercise e in availableExercises)
        {
            if (e.Id == Int32.Parse(id))
            {
                return e;
            }
        }
        return null; //todo ERROR
    }
    public string getExerciseNameById(string id)
    {
        foreach (Exercise e in availableExercises)
        {
            if (e.Id == Int32.Parse(id))
            {
                return e.name;
            }
        }
        return ""; //todo ERROR
    }

    // Retrieve information from JSON file for exercises
    // Add this to the available exercises
    public void readExerciseJSON()
    {
        var assembly = Assembly.GetExecutingAssembly();

        using (Stream stream = assembly.GetManifestResourceStream("AndroidSample.Core.exerciseInfo.json")) // Change the name of the .lic file accordingly
        {
            StreamReader sr = new StreamReader(stream);
            string json = sr.ReadToEnd();
            availableExercises = JsonConvert.DeserializeObject<List<Exercise>>(json);
        }
    }

    // Used by the exercise selection activity
    // TODO there must be a better way to do this
    public List<string>[] getExerciseInfo()
    {
        List<string>[] returnArr = new List<string>[3];

        List<string> names = new List<string>();
        List<string> imageIds = new List<string>();
        List<string> ids = new List<string>();


        foreach (Exercise e in availableExercises)
        {
            names.Add(e.name);
            imageIds.Add(e.img_name);
            ids.Add(e.Id.ToString());
        }

        returnArr[0] = names;
        returnArr[1] = imageIds;
        returnArr[2] = ids;

        return returnArr;
    }

    public List<int> getExercisesDone()
    {
        List<int> exercisesDone = new List<int>();
        if (currentSession != null)
        {
            //todo remove this once not using shortcut button

            if (currentSession.exerciseIds != null)
            {
                var lst = currentSession.exerciseIds.Split(',').ToList();
                foreach (var val in lst)
                {
                    int id;
                    bool isint = int.TryParse(val, out id);
                    if (isint == true)
                        exercisesDone.Add(id);
                }
            }
        }

        return exercisesDone;
    }
    #endregion

    #region helper methods

    /// <summary>
    /// Calculates mvc based on the processData
    /// Called post collection mvc data
    /// </summary>
    /// <returns></returns>
    public List<double> Calculate_MVC(double[][] data)
    {
        double mvc = 1; // default 

        List<double> mvcs = new List<double>();

        for (int i = 0; i < data.Length; i++) // For each channel/sensor
        {
            double sum = 0;
            double[] mvcData = data[i];
            foreach (var pt in mvcData) // for each data point
            {
                sum = sum + (pt * pt); // Add the squares of all values
            }
            mvc = sum / mvcData.Length;
            mvc = Math.Sqrt(mvc); // square root
            mvcs.Add(mvc);
        }

        return mvcs;
    }

    /// <summary>
    /// Retrieves the mvc string from the most recent session
    /// sets it as the current sessions mvc
    /// this becomes the default. The user can update the mvc values 
    /// in the MVC activity
    /// </summary>
    private void SetUpMvcs()
    {
        List<Session> allPrevSession = _database.GetItemsAsync().Result; // get last one
        if (allPrevSession.Count != 0)
        {
            Session prevSession = allPrevSession.Last();

            if (prevSession.mvcs != null)
            {
                currentSession.mvcs = prevSession.mvcs;
                del.mvcs = currentSession.getMvcs().ToArray();
            }

        }
        else
        {
            //First session
            currentSession.mvcs = "1,1"; //default to 1
            del.mvcs = currentSession.getMvcs().ToArray();
            //TODO add a force to record MVC collection before exercising
        }
    }
    /// <summary>
    /// Update the mvc values to the ones collected in this session
    /// </summary>
    public void UpdateMvcs(List<double> newMvcs)
    {
        currentSession.setMvcs(newMvcs);
        del.mvcs = newMvcs.ToArray();
        _database.SaveItemAsync(currentSession);
    }

    public void SetUpExerciseValues()
    {
        for (int i = 0; i < del.sensors.Count; i++)
        {
            exerciseMeans.Add(new List<double>());
        }
    }

    /// <summary>
    /// </summary>
    public void AddExerciseValue(int channel, double val)
    {
        exerciseMeans[channel].Add(val);
    }

    public int CalculatePerformace()
    {
        List<int> performances = new List<int>();
        foreach (var channelMeans in exerciseMeans)
        {
            int eighty = channelMeans.Where(p => p > 80).Count();
            int sixty = channelMeans.Where(p => (p > 60)).Count();
            int fourty = channelMeans.Where(p => (p > 40)).Count();
            int twenty = channelMeans.Where(p => (p > 20)).Count();


            if (eighty > (channelMeans.Count / 2)) // if more than 50 % of value are above 80
                performances.Add(5);
            else if (sixty > (channelMeans.Count / 2))
                performances.Add(4);
            else if (fourty > (channelMeans.Count / 2))
                performances.Add(3);
            else if (twenty > (channelMeans.Count / 2))
                performances.Add(2);
            else
                performances.Add(1);
        }
       return (int)Math.Round(performances.Average());
    }

    public void ClearExerciseStats()
    {
        exerciseMeans = new List<List<double>>();
    }
    #endregion
}

// Taken from 
// https://docs.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/databases
public class IndexDatabase
{
    static SQLiteAsyncConnection Database;

    public static readonly AsyncLazy<IndexDatabase> Instance = new AsyncLazy<IndexDatabase>(async () =>
    {
        var instance = new IndexDatabase();
        CreateTableResult result = await Database.CreateTableAsync<Session>();

        return instance;
    });

    public IndexDatabase()
    {
        Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
    }
    public Task<List<Session>> GetItemsAsync()
    {
        return Database.Table<Session>().ToListAsync();
    }

    public Task<List<Session>> ClearSessionTableAsync() //todo get rid of
    {
        // SQL queries are also possible
        return Database.QueryAsync<Session>("DELETE FROM [Sessions]");
    }

    public Task<Session> GetSessionAsync(int id)
    {
        return Database.Table<Session>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public Task<int> SaveItemAsync(Session item)
    {
        Session s = GetSessionAsync(item.Id).Result;
        if (s != null) //if item already exists
        {
            s.mvcs = item.mvcs;
            s.notes = item.notes;
            s.exerciseStats = item.exerciseStats;
            s.exerciseIds = item.exerciseIds;
            return Database.UpdateAsync(s);
        }
        else
        {
            return Database.InsertAsync(item);
        }
    }

    public Task<int> DeleteItemAsync(Session item)
    {
        return Database.DeleteAsync(item);
    }
}

public class AsyncLazy<T>
{
    readonly Lazy<Task<T>> instance;

    public AsyncLazy(Func<T> factory)
    {
        instance = new Lazy<Task<T>>(() => Task.Run(factory));
    }

    public AsyncLazy(Func<Task<T>> factory)
    {
        instance = new Lazy<Task<T>>(() => Task.Run(factory));
    }

    public TaskAwaiter<T> GetAwaiter()
    {
        return instance.Value.GetAwaiter();
    }
}

public static class Constants
{
    public const string DatabaseFilename = "IndexDatabase.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache |
        SQLite.SQLiteOpenFlags.ProtectionComplete;

    public static string DatabasePath
    {
        get
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, DatabaseFilename);
        }
    }
}