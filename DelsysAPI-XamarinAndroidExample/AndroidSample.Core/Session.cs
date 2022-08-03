using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AndroidSample.Core
{

    [Table("Sessions")]
    public class Session
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        
        public DateTime date { get; set; } //date of the session
        private string _exerciseIds;
        private string _exerciseStats;
        private string _notes;

        public void addExercise(Exercise exer)
        {
            _exerciseIds += exer.Id + ",";
        }

        public void addExerciseStat (Exercise e , double maxPercent)
        {
            addExercise(e);
            _exerciseStats += maxPercent + ",";
        }

        public string exerciseStats {
            get { return _exerciseStats; }
            set { _exerciseStats = value; }
        }

        public string exerciseIds
        {
            get { return _exerciseIds; }
            set { _exerciseIds = value; }
        }
        public string notes
        {
            get { return _notes; }
            set { _notes = value; //TODO add some sort of formatting
                                 }
        }
    }

    public class Exercise
    {
        static int nextId = 0;
        public int Id { get; private set; }
        public string name { get; set; } //date of the session
        public int reps { get; set; } //how many reps
        public string img_name {get; set ; } //name of image for the exercise
        public string description { get; set; } // description on how to do the exercise

        public Exercise()
        {
            Id = nextId;
            nextId++;
        }
    }
}
