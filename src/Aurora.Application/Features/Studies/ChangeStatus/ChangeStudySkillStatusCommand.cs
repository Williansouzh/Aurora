using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using MediatR;

namespace Aurora.Application.Features.Studies.ChangeStatus;

public record ChangeStudySkillStatusCommand(
    string UserId,
    string SkillId,
    string Action) : IRequest<StudySkillDto>;

public class ChangeStudySkillStatusHandler(IStudySkillRepository skills)
    : IRequestHandler<ChangeStudySkillStatusCommand, StudySkillDto>
{
    public async Task<StudySkillDto> Handle(ChangeStudySkillStatusCommand cmd, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(cmd.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        if (cmd.Action.Equals("activate", StringComparison.OrdinalIgnoreCase))
        {
            var active = await skills.GetActiveAsync(cmd.UserId, ct);
            if (active.Count >= 3 && active.All(x => x.Id != skill.Id))
            {
                throw new InvalidOperationException("Voce pode manter no maximo 3 estudos ativos. Pause uma prioridade antes de ativar outra.");
            }

            var rank = active
                .Where(x => x.Id != skill.Id)
                .Select(x => x.PriorityRank ?? 0)
                .DefaultIfEmpty(0)
                .Max() + 1;

            skill.Activate(rank);
        }
        else if (cmd.Action.Equals("pause", StringComparison.OrdinalIgnoreCase))
        {
            skill.Pause();
        }
        else
        {
            throw new InvalidOperationException("Acao invalida.");
        }

        await skills.UpdateAsync(skill, ct);
        return skill.ToDto();
    }
}

