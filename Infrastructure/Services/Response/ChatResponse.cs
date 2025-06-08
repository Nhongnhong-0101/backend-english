using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class ChatResponse
    {
        public string transcribedText {  get; set; }
        public string responseFromAI { get; set; }
    }
}
