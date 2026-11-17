using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Conversations;
using FootballManager.Api.Responses.Conversations;
using FootballManager.Application.Services;
using FootballManager.Domain.Conversations;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ConversationsController(IConversationService conversationService) : ControllerBase
{
    /// <summary>The one-to-one conversations a first-team player's menu offers now (praise or criticise his training, performances, conduct).</summary>
    [HttpGet("{playerId:int}")]
    public async Task<Results<Ok<ConversationPlayerResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        int playerId, CancellationToken cancellationToken = default)
    {
        var result = await conversationService.GetConversationsAsync(playerId, cancellationToken);
        return result.ToGetResult<ConversationPlayer, ConversationPlayerResponse>(player => player.Adapt<ConversationPlayerResponse>());
    }

    /// <summary>
    /// Opens a conversation with the player and says the chosen line of each round, then ends it, or continues the open
    /// one; only with apply=true. Opening uses the conversation up, answered or not.
    /// </summary>
    [HttpPost]
    public async Task<Results<Ok<PlayerTalkResponse>, BadRequest, NotFound, ProblemHttpResult>> TalkAsync(
        [FromBody] TalkToPlayerRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await conversationService.TalkAsync(
            request.PlayerId, request.Conversation, request.Choices, request.Continue, apply, cancellationToken);
        return result.ToGetResult<PlayerTalk, PlayerTalkResponse>(talk => talk.Adapt<PlayerTalkResponse>());
    }
}
