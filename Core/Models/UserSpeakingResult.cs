using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class UserSpeakingResult
    {
        public Guid resultId { get; set; }
        public Guid accountId { get; set; }
        public Guid questionId { get; set; }
        public double score { get; set; }
        public string topic { get; set; }
        public DateTime createdAt { get; set; }
    }
}
