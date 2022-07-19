﻿using AndroidSample.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
    public const SQLite.SQLiteOpenFlags Flags = SQLiteOpenFlags.ProtectionComplete;

    public Session currentSession;

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
            addExercise("Hamstring", 1);
            addExercise("Legraise", 2);
        }
    }
    public void deleteSessionTable()
    {
        lock (locker)
        {
            _database = new SQLiteConnection(dbPath);
            SQLiteCommand cmd = _database.CreateCommand("DROP Table 'Sessions'");
            cmd.ExecuteNonQuery();
            _database.Close();
        }
    }
    public void accessDatabase()
    {
        lock (locker)
        {
            var table = _database.Table<Session>();
            foreach (var s in table)
            {
                System.Console.WriteLine(s.Id + " " + s.date.ToString());
            }
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

    #region Session methods
    public void startSession()
    {
        currentSession = new Session();
        currentSession.date = System.DateTime.Now.ToLocalTime();
    }
    public void recordCurrentSession()
    {
        lock (locker)
        {
            _database.Insert(currentSession);
        }
    }
    public Session getSessionStats()
    {
        return currentSession;
    }
    #endregion

    #region Exercise methods
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
        catch (Exception e)
        {
            System.Console.WriteLine("Error: check that exercises table exists");
            return exercises;
        }

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
}