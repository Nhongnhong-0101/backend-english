using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Response
{
    public class KeywordsFbResponse
    {
        public string userSentence {  get; set; }
        public string evaluation { get; set; }
        public string suggestion { get; set; }
        public string explanation { get; set; }
    }
}
