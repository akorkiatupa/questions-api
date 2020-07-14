using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using netcore_api.Data.Models;

namespace netcore_api.Data
{
    public interface IQuestionCache
    {
        QuestionGetSingleResponse Get(int questionId);
        void Remove(int questionId);
        void Set(QuestionGetSingleResponse question);
    }
}
