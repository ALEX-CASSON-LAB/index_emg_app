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
        private List<Exercise> _exercises = new List<Exercise>();
        private Dictionary<string, List<double>> _exerciseStats = new Dictionary<string, List<double>>(); // exercise id, maximum percentage achieved in each rep

        public void addExercise(Exercise exer)
        {
            _exercises.Add(exer);
        }
        public void addExerciseStat(string exerciseId, double maxPercent)
        {
            List<double> percents = new List<double>();

            if (_exerciseStats.ContainsKey(exerciseId)){
                percents = _exerciseStats[exerciseId];
                percents.Add(maxPercent);
                _exerciseStats[exerciseId] = percents;
            }
            else
            {
                percents.Add(maxPercent);
                _exerciseStats.Add(exerciseId, percents);
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
        public string img_id { get; set; } //id of image for the exercise

        
    }
}
