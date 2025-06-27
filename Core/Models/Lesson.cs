using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Lesson
    {
        public Guid lessonId { get; set; }

        public Guid planId { get; set; }

        public string title { get; set; }

        public string? description { get; set; }

        public int orderIndex { get; set; }

        public string? topics { get; set; }

        public int requiredQuestions { get; set; }

        public string? example { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool isPassed { get; set; }

    }
}
