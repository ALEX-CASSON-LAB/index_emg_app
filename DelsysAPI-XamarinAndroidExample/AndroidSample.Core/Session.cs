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

        public void addExercise(Exercise exer)
        {
            _exercises.Add(exer);
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
