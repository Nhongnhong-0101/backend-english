using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class DialogueQuestionResponse
    {
        public List<DialogueLine> lines { get; set; } = new();
    }
    public class DialogueLine
    {
        public string speaker { get; set; }
        public string sentence { get; set; }
    }
}
