using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Reponses
{
    public class Meaning
    {
        public string MeaningEN { get; set; }
        public string MeaningVI { get; set; }
        public string Example { get; set; }
    }

    public class PartOfSpeech
    {
        public string Type { get; set; }
        public string AudioUrl { get; set; }
        public string Phonetic { get; set; }
        public List<Meaning> Meanings { get; set; } = new List<Meaning>();
    }

    public class VocabResponse
    {
        public string Vocab { get; set; }
        public List<PartOfSpeech> pos { get; set; } = new List<PartOfSpeech> { };
    }
}
