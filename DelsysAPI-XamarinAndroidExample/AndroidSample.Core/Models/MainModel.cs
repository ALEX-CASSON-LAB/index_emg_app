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
    }

    //TODO check if needed
    public void deleteSessionTable()
    {
        
        //_database = new SQLiteConnection(dbPath);
        //SQLiteCommand cmd = _database.CreateCommand("DROP Table 'Sessions'");
        //cmd.ExecuteNonQuery();
        //_database.Close();
        
    }

    #region Session methods
    public async void startSession()
    {
        currentSession = new Session();
        currentSession.date = System.DateTime.Now.ToLocalTime();
        await _database.SaveItemAsync(currentSession);
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

    public List<string>[] getExerciseInfo()
    {
        List<string>[] returnArr= new List<string>[2];

        List<string> names = new List<string>();
        List<string> ids = new List<string>();

        
        foreach (Exercise e in availableExercises)
        {
            names.Add(e.name);
            ids.Add(e.img_name);
        }

        returnArr[0] = names;
        returnArr[1] = ids;

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

    public Task<List<Session>> GetItemsNotDoneAsync() //todo get rid of
    {
        // SQL queries are also possible
        return Database.QueryAsync<Session>("SELECT * FROM [Session] WHERE [Done] = 0");
    }

    public Task<Session> GetItemAsync(int id)
    {
        return Database.Table<Session>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public Task<int> SaveItemAsync(Session item)
    {
        //if (item.Id != 0)
        if (GetItemAsync(item.Id) != null) //if item already exists
        {
            return Database.UpdateAsync(item);
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
        SQLite.SQLiteOpenFlags.SharedCache;

    public static string DatabasePath
    {
        get
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, DatabaseFilename);
        }
    }
}