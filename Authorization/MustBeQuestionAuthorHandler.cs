using System;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using netcore_api.Data;

namespace netcore_api.Authorization
{
    public class MustBeQuestionAuthorHandler : AuthorizationHandler<MustBeQuestionAuthorRequirement>
    {

        protected readonly IDataRepository _dataRepository;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public MustBeQuestionAuthorHandler(IDataRepository dataRepository, IHttpContextAccessor httpContextAccessor)
        {
            this._dataRepository = dataRepository;
            this._httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustBeQuestionAuthorRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {

                context.Fail();
                return Task.CompletedTask;
            }

            var questionId = this._httpContextAccessor.HttpContext.Request.RouteValues["questionId"];
            var questionIdAsInt = Convert.ToInt32(questionId);

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var question = this._dataRepository.GetQuestion(questionIdAsInt);

            if(question == null)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (question.UserId != userId)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}