using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using netcore_api.Data.Models;

namespace netcore_api.Data
{
    public interface IDataRepository
    {
        #region getQueriesRegion
        IEnumerable<QuestionGetManyResponse> GetQuestions();

        IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search);

        IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions();

        QuestionGetSingleResponse GetQuestion(int questionId);

        bool QuestionExists(int questionId);

        AnswerGetResponse GetAnswer(int answerId);
        #endregion

        #region setQueriesRegion
        QuestionGetSingleResponse PostQuestion(QuestionPostRequest question);

        QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question);

        AnswerGetResponse PostAnswer(AnswerPostRequest answer);
        #endregion

        #region deleteQueriesRegion
        void DeleteQuestion(int questionId);
        #endregion
    }
}
