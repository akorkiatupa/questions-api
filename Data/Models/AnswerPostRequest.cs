﻿using System;

namespace netcore_api.Data.Models
{
    public class AnswerPostRequest
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }
    }
}