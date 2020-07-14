using Microsoft.Extensions.Caching.Memory;
using netcore_api.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netcore_api.Data
{
    public class QuestionCache : IQuestionCache
    {
        protected MemoryCache _cache { get; set; }

        protected string GetCacheKey(int questionId) => $"Question-{questionId}";

        public QuestionCache()
        {
            // works like a stack when size limit is reached pushing will pop exceeding value from cache
            this._cache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 100 });
        }

        public QuestionGetSingleResponse Get(int questionId)
        {
            this._cache.TryGetValue(this.GetCacheKey(questionId), out QuestionGetSingleResponse question);
            return question;
        }

        public void Remove(int questionId)
        {
            this._cache.Remove(this.GetCacheKey(questionId));
        }

        public void Set(QuestionGetSingleResponse question)
        {
            // set size of the entry, when size exceeds max it will start popping out oldest cache values
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSize(1);

            this._cache.Set<QuestionGetSingleResponse>(this.GetCacheKey(question.QuestionId), question, cacheEntryOptions);
        }
    }
}
