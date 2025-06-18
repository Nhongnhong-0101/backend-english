using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class ReoderQuestionResponse
    {
        public Guid questionId { get; set; }
        public string topic { get; set; }
        public string correctSentence { get; set; }
        public List<string> shuffledWords { get; set; }
    }
}
