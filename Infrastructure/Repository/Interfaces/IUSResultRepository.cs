using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Interfaces
{
    public interface IUSResultRepository
    {
        //lay danh sach ket qua cua nguoi dung 
        public Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserAsync(Guid userId);
        //danh sach ketqua nguwoi dung theo topic 
        public Task<IEnumerable<UserSpeakingResult>> GetResultsOfUserByTopicAsync(Guid userId, string topic);

        //luu ket qua nguwoi dung
        public Task<IEnumerable<UserSpeakingResult>> SaveResultOfUserAsync(UserSpeakingResult result);

        public Task<Dictionary<string, (int total, int practiced)>> GetUserResultEachTopicAsync(Guid accountId);

    }
}
