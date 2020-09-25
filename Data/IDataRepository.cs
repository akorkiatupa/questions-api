using System.Collections.Generic;
using netcore_api.Data.Models;
using System.Threading.Tasks;

namespace netcore_api.Data
{
    // TODO: change to async methods on the repository interface
    public interface IDataRepository
    {
        #region getQueriesRegion
        IEnumerable<QuestionGetManyResponse> GetQuestions();

        IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search);

        IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions();
        Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestionsAsync();

        IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers();
        IEnumerable<QuestionGetManyResponse> GetQuestionsBySearchWithPaging( string search, int pageNumber, int pageSize );

        QuestionGetSingleResponse GetQuestion(int questionId);

        bool QuestionExists(int questionId);

        AnswerGetResponse GetAnswer(int answerId);
        #endregion

        #region setQueriesRegion
        QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question);

        QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question);

        AnswerGetResponse PostAnswer(AnswerPostFullRequest answer);
        #endregion

        #region deleteQueriesRegion
        void DeleteQuestion(int questionId);
        #endregion
    }
}
