using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class QuestionResponse
    {
        public Guid questionId { get; set; } = Guid.NewGuid();
        public string type { get; set; } //reoder, word, dialogue, keyword, prompt
        public string instructions { get; set; }
        public object data { get; set; } 
    }
}
