using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RestEase;
using RestEaseClientGeneratorConsoleApp.Examples.Cog.Models;

namespace RestEaseClientGeneratorConsoleApp.Examples.Cog.Api
{
    public interface ICogApi
    {
        /// <summary>
        /// Report reward to allocate to the top ranked action for the specified event.
        /// </summary>
        /// <param name="eventId">The event id this reward applies to.</param>
        /// <param name="contentType">The Content-Type</param>
        /// <param name="content">Reward given to a rank response.</param>
        [Post("/personalization/v1.0/events/{eventId}/reward")]
        Task RewardAsync([Path] string eventId, [Header("Content-Type")] string contentType, [Body] RewardRequest content);

        /// <summary>
        /// Report that the specified event was actually displayed to the user and a reward should be expected for it.
        /// </summary>
        /// <param name="eventId">The event id this activation applies to.</param>
        [Post("/personalization/v1.0/events/{eventId}/activate")]
        Task ActivateAsync([Path] string eventId);

        /// <summary>
        /// A personalization rank request.
        /// </summary>
        /// <param name="contentType">The Content-Type</param>
        /// <param name="content">Request a set of actions to be ranked by the personalization service.</param>
        [Post("/personalization/v1.0/rank")]
        Task<RankResponse> RankAsync([Header("Content-Type")] string contentType, [Body] RankRequest content);

        /// <summary>
        /// StatusGet (/status)
        /// </summary>
        [Get("/status")]
        Task<ContainerStatus> StatusGetAsync();
    }
}