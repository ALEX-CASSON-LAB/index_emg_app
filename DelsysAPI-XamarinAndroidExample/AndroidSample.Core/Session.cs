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
        ///*private Dictionary<string, List<double>> _exerciseStats = new Dictionary<string, List<double>>()*/; // exercise id, maximum percentage achieved in each rep
        private string _exerciseStats;
        private string _notes;

        public void addExercise(Exercise exer)
        {
            _exerciseIds += exer.Id + ",";
        }
        //public void addExerciseStat(int exerId, double maxPercent)
        //{
        //    string exerciseId = exerId.ToString(); 
        //    List<double> percents = new List<double>();

        //    if (_exerciseStats.ContainsKey(exerciseId)){
        //        percents = _exerciseStats[exerciseId];
        //        percents.Add(maxPercent);
        //        _exerciseStats[exerciseId] = percents;
        //    }
        //    else
        //    {
        //        percents.Add(maxPercent);
        //        _exerciseStats.Add(exerciseId, percents);
        //    }
        //}

        //public Dictionary<string, List<double>> exerciseStats
        //{
        //    get { return _exerciseStats; }
        //    set { _exerciseStats = value; }
        //}

        public void addExerciseStat (Exercise e , double maxPercent)
        {
            addExercise(e);
            _exerciseStats += "[ " + maxPercent + "]";
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
            get { return notes; }
            set { _notes = value; //TODO add some sort of formatting
                                 }
        }
    }

        [Table("Exercises")]
    public class Exercise
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        [MaxLength(8)]
        public string name { get; set; } //date of the session
        public int reps { get; set; } //how many reps
        public string img_name {get; set ; } //name of image for the exercise
    }
}
