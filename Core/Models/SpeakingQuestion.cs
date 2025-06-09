using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SpeakingQuestion
    {
        public Guid questionId { get; set; }
        public string sentence { get; set; }
        public string level { get; set; }
        public string topic { get; set; }
        public float[] Embedding { get; set; }

    }
}
