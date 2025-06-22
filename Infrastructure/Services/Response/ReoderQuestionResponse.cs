using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class ReoderQuestionResponse
    {
        public string sentence { get; set; }
        public List<string> shuffledWords { get; set; }
    }
}
