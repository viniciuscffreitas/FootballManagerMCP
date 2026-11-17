using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Messages;
using FootballManager.Application.Services;
using FootballManager.Domain.Messages;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class InboxController(IInboxService inboxService) : ControllerBase
{
    /// <summary>The user's inbox, read live from the running game, newest first.</summary>
    /// <param name="unreadOnly">Only the unread messages.</param>
    /// <param name="limit">At most this many messages (1-100, 20 by default).</param>
    /// <param name="skip">Start after this many, for older pages.</param>
    /// <param name="includeArticles">Add the text of the press articles staff forward.</param>
    [HttpGet]
    public async Task<Results<Ok<InboxResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int? limit = null,
        [FromQuery] int skip = 0,
        [FromQuery] bool includeArticles = false,
        CancellationToken cancellationToken = default)
    {
        var result = await inboxService.GetInboxAsync(unreadOnly, limit, skip, includeArticles, cancellationToken);
        return result.ToGetResult<Inbox, InboxResponse>(inbox => inbox.Adapt<InboxResponse>());
    }

    /// <summary>
    /// The answers the game offers to a message, read without opening it, and how many answers the inbox still needs
    /// before Continue moves on.
    /// </summary>
    [HttpGet("{messageId:int}/options")]
    public async Task<Results<Ok<MessageOptionsResponse>, BadRequest, NotFound, ProblemHttpResult>> GetOptionsAsync(
        int messageId, CancellationToken cancellationToken = default)
    {
        var result = await inboxService.GetMessageOptionsAsync(messageId, cancellationToken);
        return result.ToGetResult<MessageOptions, MessageOptionsResponse>(options => options.Adapt<MessageOptionsResponse>());
    }

    /// <summary>
    /// Answers a message with one of its options, by index, as its button would: a dry run unless apply=true. Options
    /// that open a flow are refused.
    /// </summary>
    [HttpPost("{messageId:int}/reply")]
    public async Task<Results<Ok<MessageReplyResponse>, BadRequest, NotFound, ProblemHttpResult>> ReplyAsync(
        int messageId,
        [FromQuery] int option,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await inboxService.ReplyAsync(messageId, option, apply, cancellationToken);
        return result.ToGetResult<MessageReply, MessageReplyResponse>(reply => reply.Adapt<MessageReplyResponse>());
    }
}
