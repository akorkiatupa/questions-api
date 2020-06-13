using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using netcore_api.Data;
using netcore_api.Data.Models;

namespace netcore_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;

        // IData repository added as depencency injection. As Scoped repostitory.
        public QuestionsController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return _dataRepository.GetQuestions();
            }
            else
            {
                return _dataRepository.GetQuestionsBySearch(search);
            }
        }

        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            return _dataRepository.GetUnansweredQuestions();
        }

        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);

            if (null == question)
            {
                return NotFound();
            }

            return question;
        }

        [HttpPost]
        public ActionResult<QuestionGetSingleResponse> PostQuestion(QuestionPostRequest questionPostRequest)
        {
            var savedQuestion = _dataRepository.PostQuestion(questionPostRequest);

            return CreatedAtAction(nameof(GetQuestion), new { questionId = savedQuestion.QuestionId }, savedQuestion);
        }

        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        {
            // TODO - get the question from the data repository
            // TODO - return HTTP status code 404 if the question isn't found
            // TODO - update the question model
            // TODO - call the data repository with the updated question model to update the question in the database
            // TODO - return the saved question

            var questionToUpdate = _dataRepository.GetQuestion(questionId);

            if (null == questionToUpdate)
            {
                return NotFound();
            }

            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? questionToUpdate.Title : questionPutRequest.Title;
            questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? questionToUpdate.Content : questionPutRequest.Content;

            var updatedQuestion = _dataRepository.PutQuestion(questionId, questionPutRequest);

            return updatedQuestion;
        }

        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);

            if(null == question)
            {
                return NotFound();
            }

            _dataRepository.DeleteQuestion(questionId);

            return NoContent();
        }

        [HttpPost("answer")]
        public ActionResult<AnswerGetResponse> PostAnswer(AnswerPostRequest answerPostRequest)
        {
            var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId);
            if (!questionExists)
            {
                return NotFound();
            }
            var savedAnswer = _dataRepository.PostAnswer(answerPostRequest);
            return savedAnswer;
        }
    }
}
