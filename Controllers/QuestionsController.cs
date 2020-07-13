using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using netcore_api.Data;
using netcore_api.Data.Models;
using Microsoft.AspNetCore.SignalR;
using netcore_api.Hubs;

namespace netcore_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHubContext<QuestionsHub> _questionsHubContext;

        // IData repository added as depencency injection. As Scoped repostitory.
        public QuestionsController(IDataRepository dataRepository, IHubContext<QuestionsHub> questionHubContext)
        {
            _dataRepository = dataRepository;
            _questionsHubContext = questionHubContext;
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
            var savedQuestion = _dataRepository.PostQuestion(new QuestionPostFullRequest 
            { 
                Title = questionPostRequest.Title,
                Content = questionPostRequest.Content,
                UserId = "1",
                UserName ="bob.test@test.com",
                Created = DateTime.UtcNow
            });

            return CreatedAtAction(nameof(GetQuestion), new { questionId = savedQuestion.QuestionId }, savedQuestion);
        }

        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        {

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
            var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
            if (!questionExists)
            {
                return NotFound();
            }
            var savedAnswer = _dataRepository.PostAnswer(new AnswerPostFullRequest 
            {
                Content = answerPostRequest.Content,
                QuestionId = answerPostRequest.QuestionId.Value,
                UserId = "1",
                UserName = "bob.test@test.com",
                Created = DateTime.UtcNow

            });

            _questionsHubContext.Clients.Group($"Questions-{answerPostRequest.QuestionId.Value}")
                .SendAsync("ReceiveQuestion", _dataRepository.GetQuestion(answerPostRequest.QuestionId.Value));

            return savedAnswer;
        }
    }
}
