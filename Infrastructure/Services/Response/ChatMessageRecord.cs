using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class ChatMessageRecord
    {
        public string role { get; set; } // "user"  "assistant"
        public string message { get; set; }
        public DateTime timestamp { get; set; }
        public int level { get; set; }
    }
}
