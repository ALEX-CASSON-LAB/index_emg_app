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
        setUpMvcs();
        _database.SaveItemAsync(currentSession).Wait();
    }
    public async void recordCurrentSession()
    {
        await _database.SaveItemAsync(currentSession);
    }
    public Session getCurrentSession()
    {
        return currentSession;
    }

    public List<Session> getAllSessions()
    {
        List<Session> allSessions = _database.GetItemsAsync().Result;
        return allSessions;
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
        List<string>[] returnArr= new List<string>[3];

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
    public static double[] fullWaveRectification(double[] data)
    {
        double[] rectData = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
            rectData[i] = Math.Abs(data[i]);
        return rectData;
    }

    /// <summary>
    /// Retrieves the mvc string from the most recent session
    /// sets it as the current sessions mvc
    /// this becomes the default. The user can update the mvc values 
    /// in the MVC activity
    /// </summary>
    private void setUpMvcs()
    {
        List<double> prevMvcs = new List<double>();

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
        //First session
        currentSession.mvcs = "1,1"; //default to 1
        del.mvcs = currentSession.getMvcs().ToArray();
        //TODO add a force to record MVC collection before exercising

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

    public Task<Session> GetItemAsync(int id)
    {
        return Database.Table<Session>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public Task<int> SaveItemAsync(Session item)
    {
        Session s = GetItemAsync(item.Id).Result;
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