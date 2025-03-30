using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class WordSet
    {
        public Guid wordsetId {get; set;}
        public string nameSet {get; set;}
        public string imageUrl {get; set;}
        public DateTime updatedAt {get; set;}
        public Guid accountId { get; set; }
        public bool isStar { get; set; }
         
        public WordSet() { }
        public WordSet(Guid wordsetId, string name, string imageUrl, DateTime updatedAt, Guid acoountId, bool isStar)
        {
            this.wordsetId = wordsetId;
            this.nameSet = name;
            this.imageUrl = imageUrl;
            this.updatedAt = updatedAt;
            this.accountId = accountId;
            this.isStar = isStar;
        }
    }
}
