using Microsoft.AspNetCore.Authorization;

namespace netcore_api.Authorization
{
    public class MustBeQuestionAuthorRequirement : IAuthorizationRequirement
    {
        public MustBeQuestionAuthorRequirement()
        {

        }
    }
}