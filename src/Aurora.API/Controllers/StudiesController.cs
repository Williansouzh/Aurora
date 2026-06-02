using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Studies.AssessPriority;
using Aurora.Application.Features.Studies.ChangeStatus;
using Aurora.Application.Features.Studies.CompleteReview;
using Aurora.Application.Features.Studies.CompletePracticeTask;
using Aurora.Application.Features.Studies.CreatePracticeTask;
using Aurora.Application.Features.Studies.Common;
using Aurora.Application.Features.Studies.CreateResource;
using Aurora.Application.Features.Studies.CreateSkill;
using Aurora.Application.Features.Studies.CreateSession;
using Aurora.Application.Features.Studies.CreateTopic;
using Aurora.Application.Features.Studies.FinishSession;
using Aurora.Application.Features.Studies.GetDashboard;
using Aurora.Application.Features.Studies.GetDueReviews;
using Aurora.Application.Features.Studies.GetPracticeTasks;
using Aurora.Application.Features.Studies.GetResources;
using Aurora.Application.Features.Studies.GetSkills;
using Aurora.Application.Features.Studies.GetSessions;
using Aurora.Application.Features.Studies.GetTopics;
using Aurora.Application.Features.Studies.UpdateResource;
using Aurora.Application.Features.Studies.UpdateTopic;
using Aurora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Studies), Route("api/studies")]
public class StudiesController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() =>
        Ok(new ApiResponse<StudyDashboardDto>(true, await sender.Send(new GetStudiesDashboardQuery(user.UserId))));

    [HttpGet("skills")]
    public async Task<IActionResult> Skills([FromQuery] StudySkillStatus? status) =>
        Ok(new ApiResponse<List<StudySkillDto>>(true, await sender.Send(new GetStudySkillsQuery(user.UserId, status))));

    [HttpPost("skills")]
    public async Task<IActionResult> CreateSkill(CreateStudySkillCommand req) =>
        Ok(new ApiResponse<StudySkillDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPost("skills/{id}/priority-assessment")]
    public async Task<IActionResult> AssessPriority(string id, AssessStudyPriorityCommand req) =>
        Ok(new ApiResponse<StudyPriorityAssessmentDto>(true,
            await sender.Send(req with { UserId = user.UserId, SkillId = id })));

    [HttpPatch("skills/{id}/activate")]
    public async Task<IActionResult> Activate(string id) =>
        Ok(new ApiResponse<StudySkillDto>(true,
            await sender.Send(new ChangeStudySkillStatusCommand(user.UserId, id, "activate"))));

    [HttpPatch("skills/{id}/pause")]
    public async Task<IActionResult> Pause(string id) =>
        Ok(new ApiResponse<StudySkillDto>(true,
            await sender.Send(new ChangeStudySkillStatusCommand(user.UserId, id, "pause"))));

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions([FromQuery] StudySessionStatus? status, [FromQuery] int limit = 12) =>
        Ok(new ApiResponse<List<StudySessionDto>>(true,
            await sender.Send(new GetStudySessionsQuery(user.UserId, status, limit))));

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession(CreateStudySessionCommand req) =>
        Ok(new ApiResponse<StudySessionDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPatch("sessions/{id}/finish")]
    public async Task<IActionResult> FinishSession(string id, FinishStudySessionCommand req) =>
        Ok(new ApiResponse<StudySessionDto>(true,
            await sender.Send(req with { UserId = user.UserId, SessionId = id })));

    [HttpGet("reviews/due")]
    public async Task<IActionResult> DueReviews([FromQuery] int limit = 10) =>
        Ok(new ApiResponse<List<StudyReviewDto>>(true,
            await sender.Send(new GetDueStudyReviewsQuery(user.UserId, limit))));

    [HttpPatch("reviews/{id}/complete")]
    public async Task<IActionResult> CompleteReview(string id, CompleteStudyReviewCommand req) =>
        Ok(new ApiResponse<StudyReviewDto>(true,
            await sender.Send(req with { UserId = user.UserId, ReviewId = id })));

    [HttpGet("skills/{skillId}/topics")]
    public async Task<IActionResult> Topics(string skillId) =>
        Ok(new ApiResponse<List<StudyTopicDto>>(true,
            await sender.Send(new GetStudyTopicsQuery(user.UserId, skillId))));

    [HttpPost("skills/{skillId}/topics")]
    public async Task<IActionResult> CreateTopic(string skillId, CreateStudyTopicCommand req) =>
        Ok(new ApiResponse<StudyTopicDto>(true,
            await sender.Send(req with { UserId = user.UserId, SkillId = skillId })));

    [HttpPut("topics/{id}")]
    public async Task<IActionResult> UpdateTopic(string id, UpdateStudyTopicCommand req) =>
        Ok(new ApiResponse<StudyTopicDto>(true,
            await sender.Send(req with { UserId = user.UserId, TopicId = id })));

    [HttpGet("skills/{skillId}/resources")]
    public async Task<IActionResult> Resources(string skillId) =>
        Ok(new ApiResponse<List<StudyResourceDto>>(true,
            await sender.Send(new GetStudyResourcesQuery(user.UserId, skillId))));

    [HttpPost("skills/{skillId}/resources")]
    public async Task<IActionResult> CreateResource(string skillId, CreateStudyResourceCommand req) =>
        Ok(new ApiResponse<StudyResourceDto>(true,
            await sender.Send(req with { UserId = user.UserId, SkillId = skillId })));

    [HttpPut("resources/{id}")]
    public async Task<IActionResult> UpdateResource(string id, UpdateStudyResourceCommand req) =>
        Ok(new ApiResponse<StudyResourceDto>(true,
            await sender.Send(req with { UserId = user.UserId, ResourceId = id })));

    [HttpGet("skills/{skillId}/practice-tasks")]
    public async Task<IActionResult> PracticeTasks(
        string skillId,
        [FromQuery] StudyPracticeStatus? status,
        [FromQuery] int limit = 20) =>
        Ok(new ApiResponse<List<StudyPracticeTaskDto>>(true,
            await sender.Send(new GetStudyPracticeTasksQuery(user.UserId, skillId, status, limit))));

    [HttpPost("skills/{skillId}/practice-tasks")]
    public async Task<IActionResult> CreatePracticeTask(string skillId, CreateStudyPracticeTaskCommand req) =>
        Ok(new ApiResponse<StudyPracticeTaskDto>(true,
            await sender.Send(req with { UserId = user.UserId, SkillId = skillId })));

    [HttpPatch("practice-tasks/{id}/complete")]
    public async Task<IActionResult> CompletePracticeTask(string id, CompleteStudyPracticeTaskCommand req) =>
        Ok(new ApiResponse<StudyPracticeTaskDto>(true,
            await sender.Send(req with { UserId = user.UserId, PracticeTaskId = id })));
}
