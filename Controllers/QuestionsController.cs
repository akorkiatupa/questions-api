﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using netcore_api.Data;
using netcore_api.Data.Models;
using Microsoft.AspNetCore.SignalR;
using netcore_api.Hubs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

namespace netcore_api.Controllers
{
    // TODO: Change all controller actions to asyncronous returning tasks
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHubContext<QuestionsHub> _questionsHubContext;
        private readonly IQuestionCache _questionCache;

        private readonly IHttpClientFactory _clientFactory;
        private readonly string _auth0UserInfo;

        // IData repository added as scoped repostitory depencency injection.
        public QuestionsController(
            IDataRepository dataRepository,
            IHubContext<QuestionsHub> questionHubContext,
            IQuestionCache questionCache,
            IHttpClientFactory clientFactory,
            IConfiguration configuration)
        {
            this._dataRepository = dataRepository;
            this._questionsHubContext = questionHubContext;
            this._questionCache = questionCache;
            this._clientFactory = clientFactory;
            this._auth0UserInfo = $"{configuration["Auth0:Authority"]}userinfo";
        }

        // Get username from external Auth0 service
        private async Task<string> GetAuth0UserName() 
        {
            var request = new HttpRequestMessage(HttpMethod.Get, this._auth0UserInfo);

            request.Headers.Add("Authorization", this.Request.Headers["Authorization"].First());

            var httpClient = this._clientFactory.CreateClient();

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();

                var user = JsonSerializer.Deserialize<User>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                return user.Name;
            }else
            {
                return "";
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search, bool includeAnswers = false, int page = 1, int pageSize=20)
        {
            if (string.IsNullOrEmpty(search))
            {
                if (includeAnswers)
                {
                    return this._dataRepository.GetQuestionsWithAnswers();
                }
                return this._dataRepository.GetQuestions();
            }
            else
            {   
                return this._dataRepository.GetQuestionsBySearchWithPaging(search, page, pageSize);
            }
        }

        [AllowAnonymous]
        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            return this._dataRepository.GetUnansweredQuestions();
        }

        [AllowAnonymous]
        [HttpGet("answered")]
        public IEnumerable<QuestionGetManyResponse> GetAnsweredQuestions()
        {
            return this._dataRepository.GetQuestionsWithAnswers().Where(question => question.Answers.Count() > 0);
        }

        [AllowAnonymous]
        [HttpGet("unansweredasync")]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestionsAsync()
        {
            return await this._dataRepository.GetUnansweredQuestionsAsync();
        }

        [AllowAnonymous]
        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = this._questionCache.Get(questionId);

            if (question == null)
            {
                question = this._dataRepository.GetQuestion(questionId);

                if (null == question)
                {
                    return this.NotFound();
                }

                this._questionCache.Set(question);
            }

            return question;
        }

        [HttpPost]
        public async Task<ActionResult<QuestionGetSingleResponse>> PostQuestionAsync(QuestionPostRequest questionPostRequest)
        {
            var savedQuestion = this._dataRepository.PostQuestion(new QuestionPostFullRequest 
            { 
                Title = questionPostRequest.Title,
                Content = questionPostRequest.Content,
                UserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserName = await this.GetAuth0UserName(),
                Created = DateTime.UtcNow
            });

            this._questionCache.Set(savedQuestion);

            return this.CreatedAtAction(nameof(GetQuestion), new { questionId = savedQuestion.QuestionId }, savedQuestion);
        }

        [Authorize(Policy = "MustBeQuestionAuthor")]
        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        {

            var questionToUpdate = this._dataRepository.GetQuestion(questionId);

            if (null == questionToUpdate)
            {
                return this.NotFound();
            }

            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? questionToUpdate.Title : questionPutRequest.Title;
            questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? questionToUpdate.Content : questionPutRequest.Content;

            var updatedQuestion = this._dataRepository.PutQuestion(questionId, questionPutRequest);

            this._questionCache.Remove(updatedQuestion.QuestionId);

            return updatedQuestion;
        }

        [Authorize(Policy = "MustBeQuestionAuthor")]
        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = this._dataRepository.GetQuestion(questionId);

            if(null == question)
            {
                return this.NotFound();
            }

            this._dataRepository.DeleteQuestion(questionId);
            this._questionCache.Remove(questionId);

            return this.NoContent();
        }

        
        [HttpPost("answer")]       
        public async Task<ActionResult<AnswerGetResponse>> PostAnswerAsync(AnswerPostRequest answerPostRequest)
        {
            var questionExists = this._dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
            if (!questionExists)
            {
                return this.NotFound();
            }
            var savedAnswer = this._dataRepository.PostAnswer(new AnswerPostFullRequest 
            {
                Content = answerPostRequest.Content,
                QuestionId = answerPostRequest.QuestionId.Value,
                UserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                UserName = await this.GetAuth0UserName(),
                Created = DateTime.UtcNow
            });
            this._questionCache.Remove(answerPostRequest.QuestionId.Value);

            await this._questionsHubContext.Clients.Group($"Questions-{answerPostRequest.QuestionId.Value}")
                .SendAsync("ReceiveQuestion", this._dataRepository.GetQuestion(answerPostRequest.QuestionId.Value));

            return savedAnswer;
        }
    }
}
