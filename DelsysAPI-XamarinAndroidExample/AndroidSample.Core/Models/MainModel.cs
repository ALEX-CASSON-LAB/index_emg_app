using AndroidSample.Core;
using SQLite;
using System;
using System.Collections.Generic;

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
    object locker = new object(); // class level private field  // use this locker for all the database work

    private Delsys _del;
    private double[][] _data;
    public double mvc { get; set; }

    public string dbPath { get; set; }
    private SQLiteConnection _database;
    Session currentSession;
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

    public void setupDatabase()
    {
        lock (locker)
        {
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Session>();
            //TODO add try catch or soemthing idk for the sql connection
            // Care must be taken to avoid a deadlock situation by ensuring that the work inside the lock
            // clause is kept simple and does not call out to other methods that may also take a lock!
        }
        lock (locker)
        {
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<Exercise>();
            addExercise("Hamstring",1);
            addExercise("Legraise",2);
        }
    }

    public void accessDatabase()
    {
        /*****use this for database stuff****/
        
        lock (locker)
        {
            var table = _database.Table<Session>();
            foreach (var s in table)
            {
                System.Console.WriteLine(s.Id + " " + s.date.ToString());
            }
            //var stock = _database.Get<Stock>(0); //primary key id of 0
            // Care must be taken to avoid a deadlock situation by ensuring that the work inside the lock
            // clause is kept simple and does not call out to other methods that may also take a lock!
        }
        /********************************/
    }

    public void addExercise(string exercise_name, int reps)
    {
        lock (locker)
        { 
            var newExercise = new Exercise();
            newExercise.name = exercise_name;
            newExercise.reps = reps;
            _database.Insert(newExercise);
            // Care must be taken to avoid a deadlock situation by ensuring that the work inside the lock
            // clause is kept simple and does not call out to other methods that may also take a lock!
        }
    }

    public void addSession()
    {
        currentSession = new Session();
        currentSession.date = System.DateTime.Now.ToLocalTime();
        _database.Insert(currentSession);
    }

    public List<Exercise> getExercises()
    {

        List<Exercise> exercises = new List<Exercise>();
        try
        {
            lock (locker)
            {
                var table = _database.Table<Exercise>();
                foreach (var e in table)
                {
                    exercises.Add(e);
                }
                return exercises;
                // Care must be taken to avoid a deadlock situation by ensuring that the work inside the lock
                // clause is kept simple and does not call out to other methods that may also take a lock!
            }
        }
        catch (Exception e ){
            System.Console.WriteLine("Error: check that exercises table exists");
            return exercises;
        }
        
    }
   
}