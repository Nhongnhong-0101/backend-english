using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class WordSet
    {
        private Guid wordsetId;
        private string name;
        private string imageUrl;
        private DateTime updatedAt;
        private Guid userId;

        public WordSet(Guid wordsetId, string name, string imageUrl, DateTime updatedAt, Guid userId)
        {
            this.wordsetId = wordsetId;
            this.name = name;
            this.imageUrl = imageUrl;
            this.updatedAt = updatedAt;
            this.userId = userId;
        }
    }
}
