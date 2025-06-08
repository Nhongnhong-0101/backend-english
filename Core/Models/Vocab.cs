using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Vocab
    {
        [Column("vocab_id")]
        public Guid vocabId { get; set; }
        public string vocab { get; set; }
        public string primaryMeaningVi { get; set; }
        public string primaryMeaningEn { get; set; }
        public string phonetic { get; set; }
        public string audioUrl { get; set; }
        public string partOfSpeech { get; set; }

        public Vocab()
        {

        }
        public Vocab(Guid vocabId, string vocab, string primaryMeaningVi, string primaryMeaningEn, string phonetic, string audioUrl, string pos)
        {
            this.vocabId = vocabId;
            this.vocab = vocab;
            this.primaryMeaningVi = primaryMeaningVi;
            this.primaryMeaningEn = primaryMeaningEn;
            this.phonetic = phonetic;
            this.audioUrl = audioUrl;
            this.partOfSpeech = pos;
        }
    }

}
