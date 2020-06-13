using System;
using System.ComponentModel.DataAnnotations;

namespace netcore_api.Data.Models
{
    public class AnswerPostRequest
    {
        [Required]
        public int? QuestionId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
