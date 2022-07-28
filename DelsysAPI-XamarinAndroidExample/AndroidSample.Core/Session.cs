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
        [MaxLength(8)]
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

        [Table("Exercises")]
    public class Exercise
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string name { get; set; } //date of the session
        public int reps { get; set; } //how many reps
        public string img_name {get; set ; } //name of image for the exercise
    }
}
