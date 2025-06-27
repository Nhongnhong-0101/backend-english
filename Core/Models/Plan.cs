using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Plan
    {
        public Guid planId { get; set; }

        public string description { get; set; }

        public string? purpose { get; set; }

        public int orderIndex { get; set; }

        public int requiredQuestions { get; set; }

        public string skill { get; set; } // grammar, speaking

        public int? totalLessons { get; set; }

        public DateTime createdAt { get; set; }

        public bool isDone { get; set; }

    }
}
