import {
  BookOpen,
  Brain,
  CheckCircle2,
  ClipboardList,
  Clock,
  History,
  Pause,
  Play,
  Plus,
  SlidersHorizontal,
  Target,
} from 'lucide-react';
import { useEffect, useState } from 'react';
import { Badge } from '../components/ui/badge';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Progress } from '../components/ui/progress';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const STATUS = { 1: 'Backlog', 2: 'Ativo', 3: 'Pausado', 4: 'Concluido', 5: 'Arquivado' };
const STATUS_COLORS = {
  1: 'bg-slate-100 text-slate-700',
  2: 'bg-emerald-100 text-emerald-700',
  3: 'bg-amber-100 text-amber-700',
  4: 'bg-violet-100 text-violet-700',
  5: 'bg-slate-100 text-slate-500',
};
const CATEGORIES = {
  1: 'Idiomas',
  2: 'Tecnologia',
  3: 'Matematica',
  4: 'Comunicacao',
  5: 'Arte/Musica',
  6: 'Saude/Corpo',
  7: 'Design',
  8: 'Escola/Faculdade',
  9: 'Carreira',
  10: 'Outros',
};
const STAGES = {
  1: 'Obter',
  2: 'Organizar',
  3: 'Memorizar',
  4: 'Aplicar',
  5: 'Ensinar',
};
const SESSION_STATUS = { 1: 'Planejada', 2: 'Concluida', 3: 'Cancelada' };
const SESSION_STATUS_COLORS = {
  1: 'bg-blue-100 text-blue-700',
  2: 'bg-emerald-100 text-emerald-700',
  3: 'bg-slate-100 text-slate-500',
};
const REVIEW_RESULTS = {
  1: 'Repetir',
  2: 'Dificil',
  3: 'Bom',
  4: 'Facil',
};
const TOPIC_STATUS = { 1: 'Nao iniciado', 2: 'Em andamento', 3: 'Revisao', 4: 'Concluido', 5: 'Arquivado' };
const RESOURCE_TYPES = {
  1: 'Curso',
  2: 'Livro',
  3: 'Video',
  4: 'Documentacao',
  5: 'Artigo',
  6: 'Lista exercicios',
  7: 'Mentor',
  8: 'Outro',
};
const RESOURCE_STATUS = { 1: 'Planejado', 2: 'Em andamento', 3: 'Concluido', 4: 'Arquivado' };
const PRACTICE_STATUS = { 1: 'Planejada', 2: 'Em andamento', 3: 'Concluida', 4: 'Cancelada' };
const PRACTICE_STATUS_COLORS = {
  1: 'bg-blue-100 text-blue-700',
  2: 'bg-amber-100 text-amber-700',
  3: 'bg-emerald-100 text-emerald-700',
  4: 'bg-slate-100 text-slate-500',
};

export function StudiesPage({ api }) {
  const [statusFilter, setStatusFilter] = useState('_all');
  const [showSkillForm, setShowSkillForm] = useState(false);
  const [showSessionForm, setShowSessionForm] = useState(false);
  const [assessmentSkill, setAssessmentSkill] = useState(null);
  const [finishSession, setFinishSession] = useState(null);
  const [completeReview, setCompleteReview] = useState(null);
  const [selectedSkillId, setSelectedSkillId] = useState('');
  const [showTopicForm, setShowTopicForm] = useState(false);
  const [showResourceForm, setShowResourceForm] = useState(false);
  const [showPracticeForm, setShowPracticeForm] = useState(false);
  const [completePractice, setCompletePractice] = useState(null);

  const dashboard = useData(() => api.get('/api/studies/dashboard'), []);
  const skills = useData(
    () => api.get(`/api/studies/skills${statusFilter !== '_all' ? `?status=${statusFilter}` : ''}`),
    [statusFilter],
  );
  const sessions = useData(() => api.get('/api/studies/sessions?limit=10'), []);
  const reviews = useData(() => api.get('/api/studies/reviews/due?limit=10'), []);
  const topics = useData(
    () => selectedSkillId ? api.get(`/api/studies/skills/${selectedSkillId}/topics`) : Promise.resolve([]),
    [selectedSkillId],
  );
  const resources = useData(
    () => selectedSkillId ? api.get(`/api/studies/skills/${selectedSkillId}/resources`) : Promise.resolve([]),
    [selectedSkillId],
  );
  const practices = useData(
    () => selectedSkillId ? api.get(`/api/studies/skills/${selectedSkillId}/practice-tasks?limit=20`) : Promise.resolve([]),
    [selectedSkillId],
  );

  const reload = () => {
    dashboard.reload();
    skills.reload();
    sessions.reload();
    reviews.reload();
    topics.reload();
    resources.reload();
    practices.reload();
  };

  const activate = async (id) => {
    await api.patch(`/api/studies/skills/${id}/activate`);
    reload();
  };

  const pause = async (id) => {
    await api.patch(`/api/studies/skills/${id}/pause`);
    reload();
  };

  const data = dashboard.data ?? {};
  const list = skills.data ?? [];
  const sessionList = sessions.data ?? [];
  const reviewList = reviews.data ?? [];
  const activeSkills = data.activePriorities ?? [];
  const trailSkills = list.length > 0 ? list : activeSkills;
  const currentSkillId = selectedSkillId || trailSkills[0]?.id || '';

  useEffect(() => {
    if (!selectedSkillId && currentSkillId) setSelectedSkillId(currentSkillId);
  }, [selectedSkillId, currentSkillId]);

  if (dashboard.loading || skills.loading || sessions.loading || reviews.loading) return <StudiesSkeleton />;
  if (dashboard.error) return <p className="text-sm text-destructive">{dashboard.error}</p>;
  if (skills.error) return <p className="text-sm text-destructive">{skills.error}</p>;
  if (sessions.error) return <p className="text-sm text-destructive">{sessions.error}</p>;
  if (reviews.error) return <p className="text-sm text-destructive">{reviews.error}</p>;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <BookOpen className="h-5 w-5 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Estudos</h1>
            <p className="text-sm text-muted-foreground">Priorize 3 habilidades e estude com metodo.</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="h-8 w-36 text-sm"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="_all">Todas</SelectItem>
              <SelectItem value="1">Backlog</SelectItem>
              <SelectItem value="2">Ativas</SelectItem>
              <SelectItem value="3">Pausadas</SelectItem>
            </SelectContent>
          </Select>
          <Button size="sm" onClick={() => setShowSkillForm(true)}>
            <Plus className="mr-1 h-4 w-4" /> Nova habilidade
          </Button>
          <Button size="sm" variant="outline" onClick={() => setShowSessionForm(true)} disabled={activeSkills.length === 0}>
            <Clock className="mr-1 h-4 w-4" /> Nova sessao
          </Button>
        </div>
      </div>

      <StudyDashboard data={data} />

      <StudyReviewsSection reviews={reviewList} onComplete={setCompleteReview} />

      <StudyTrailSection
        skills={trailSkills}
        selectedSkillId={currentSkillId}
        onSelectSkill={setSelectedSkillId}
        topics={topics.data ?? []}
        resources={resources.data ?? []}
        practices={practices.data ?? []}
        loading={topics.loading || resources.loading || practices.loading}
        onAddTopic={() => setShowTopicForm(true)}
        onAddResource={() => setShowResourceForm(true)}
        onAddPractice={() => setShowPracticeForm(true)}
        onCompletePractice={setCompletePractice}
      />

      <StudySessionsSection
        sessions={sessionList}
        onFinish={setFinishSession}
        onCreate={() => setShowSessionForm(true)}
        canCreate={activeSkills.length > 0}
      />

      {list.length === 0 ? (
        <EmptyStudies onAdd={() => setShowSkillForm(true)} />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {list.map((skill) => (
            <StudySkillCard
              key={skill.id}
              skill={skill}
              onAssess={() => setAssessmentSkill(skill)}
              onActivate={() => activate(skill.id)}
              onPause={() => pause(skill.id)}
            />
          ))}
        </div>
      )}

      {showSkillForm && (
        <StudySkillForm api={api} onClose={() => setShowSkillForm(false)} onCreated={reload} />
      )}
      {showSessionForm && (
        <StudySessionForm
          api={api}
          skills={activeSkills}
          onClose={() => setShowSessionForm(false)}
          onCreated={reload}
        />
      )}
      {finishSession && (
        <FinishSessionModal
          api={api}
          session={finishSession}
          onClose={() => setFinishSession(null)}
          onFinished={reload}
        />
      )}
      {completeReview && (
        <CompleteReviewModal
          api={api}
          review={completeReview}
          onClose={() => setCompleteReview(null)}
          onCompleted={reload}
        />
      )}
      {showTopicForm && currentSkillId && (
        <StudyTopicForm
          api={api}
          skillId={currentSkillId}
          onClose={() => setShowTopicForm(false)}
          onCreated={() => { topics.reload(); setShowTopicForm(false); }}
        />
      )}
      {showResourceForm && currentSkillId && (
        <StudyResourceForm
          api={api}
          skillId={currentSkillId}
          onClose={() => setShowResourceForm(false)}
          onCreated={() => { resources.reload(); setShowResourceForm(false); }}
        />
      )}
      {showPracticeForm && currentSkillId && (
        <StudyPracticeForm
          api={api}
          skillId={currentSkillId}
          topics={topics.data ?? []}
          onClose={() => setShowPracticeForm(false)}
          onCreated={() => { practices.reload(); dashboard.reload(); setShowPracticeForm(false); }}
        />
      )}
      {completePractice && (
        <CompletePracticeModal
          api={api}
          practice={completePractice}
          onClose={() => setCompletePractice(null)}
          onCompleted={reload}
        />
      )}
      {assessmentSkill && (
        <PriorityAssessmentModal
          api={api}
          skill={assessmentSkill}
          onClose={() => setAssessmentSkill(null)}
          onSaved={reload}
        />
      )}
    </div>
  );
}

function StudyDashboard({ data }) {
  const activeCount = data.activeSkills ?? 0;
  const weeklyHours = ((data.weeklyTimeBudgetMinutes ?? 0) / 60).toFixed(1);
  const completedHours = ((data.completedMinutesThisWeek ?? 0) / 60).toFixed(1);

  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-6">
      <MetricCard icon={Target} label="Prioridades ativas" value={`${activeCount}/3`} />
      <MetricCard icon={Brain} label="Habilidades no radar" value={data.totalSkills ?? 0} />
      <MetricCard icon={Clock} label="Planejado/semana" value={`${weeklyHours}h`} />
      <MetricCard icon={CheckCircle2} label="Estudado na semana" value={`${completedHours}h`} />
      <MetricCard icon={RotateIcon} label="Revisoes vencidas" value={data.dueReviews ?? 0} />
      <MetricCard icon={History} label="Revisoes na semana" value={data.completedReviewsThisWeek ?? 0} />
      <MetricCard icon={ClipboardList} label="Praticas abertas" value={data.openPracticeTasks ?? 0} />
      <MetricCard icon={CheckCircle2} label="Praticas na semana" value={data.completedPracticeTasksThisWeek ?? 0} />
    </div>
  );
}

function RotateIcon(props) {
  return <History {...props} />;
}

function MetricCard({ icon: Icon, label, value }) {
  return (
    <Card>
      <CardContent className="p-4">
        <div className="mb-2 flex items-center justify-between">
          <p className="text-xs font-medium text-muted-foreground">{label}</p>
          <Icon className="h-4 w-4 text-primary" />
        </div>
        <p className="text-2xl font-bold tabular-nums">{value}</p>
      </CardContent>
    </Card>
  );
}

function StudySkillCard({ skill, onAssess, onActivate, onPause }) {
  const active = skill.status === 2;
  const scorePct = Math.max(0, Math.min(100, Math.round(((skill.priorityScore ?? 0) / 45) * 100)));

  return (
    <Card className="transition-shadow hover:shadow-md">
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <CardTitle className="truncate text-base font-semibold">{skill.title}</CardTitle>
            <div className="mt-1 flex flex-wrap items-center gap-1.5">
              <span className={cn('rounded-full px-1.5 py-0.5 text-[11px] font-medium', STATUS_COLORS[skill.status])}>
                {STATUS[skill.status]}
              </span>
              <span className="text-[11px] text-muted-foreground">{CATEGORIES[skill.category]}</span>
              {skill.priorityRank && <Badge variant="outline" className="text-[11px]">Top {skill.priorityRank}</Badge>}
            </div>
          </div>
          <div className="flex shrink-0 items-center gap-1">
            <button className="p-1 text-muted-foreground hover:text-primary" onClick={onAssess} title="Avaliar prioridade">
              <SlidersHorizontal className="h-4 w-4" />
            </button>
            {active ? (
              <button className="p-1 text-muted-foreground hover:text-foreground" onClick={onPause} title="Pausar">
                <Pause className="h-4 w-4" />
              </button>
            ) : (
              <button className="p-1 text-muted-foreground hover:text-foreground" onClick={onActivate} title="Ativar">
                <Play className="h-4 w-4" />
              </button>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {skill.purpose && <p className="line-clamp-2 text-sm text-muted-foreground">{skill.purpose}</p>}
        <div>
          <div className="mb-1 flex justify-between text-xs text-muted-foreground">
            <span>Score de prioridade</span>
            <span className="font-semibold tabular-nums">{Number(skill.priorityScore ?? 0).toFixed(1)}</span>
          </div>
          <Progress value={scorePct} className="h-2" indicatorClassName={scorePct >= 70 ? 'bg-emerald-500' : 'bg-blue-500'} />
        </div>
        <div className="flex flex-wrap gap-3 text-xs text-muted-foreground">
          <span>{Math.round((skill.weeklyTimeBudgetMinutes ?? 0) / 60)}h/semana</span>
          {skill.currentLevel && <span>Atual: {skill.currentLevel}</span>}
          {skill.targetLevel && <span>Meta: {skill.targetLevel}</span>}
        </div>
      </CardContent>
    </Card>
  );
}

function StudyReviewsSection({ reviews, onComplete }) {
  return (
    <section className="space-y-3">
      <div>
        <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">Revisoes espacadas</h2>
        <p className="text-xs text-muted-foreground">Prioridade do dia: recupere da memoria antes de consumir conteudo novo.</p>
      </div>

      {reviews.length === 0 ? (
        <Card>
          <CardContent className="flex items-center gap-3 p-4">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-emerald-100">
              <CheckCircle2 className="h-4 w-4 text-emerald-700" />
            </div>
            <div>
              <p className="text-sm font-semibold">Nenhuma revisao vencida</p>
              <p className="text-xs text-muted-foreground">Quando uma sessao for finalizada, o Aurora agenda a primeira revisao para D+1.</p>
            </div>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {reviews.map((review) => (
            <StudyReviewCard key={review.id} review={review} onComplete={() => onComplete(review)} />
          ))}
        </div>
      )}
    </section>
  );
}

function StudyTrailSection({
  skills,
  selectedSkillId,
  onSelectSkill,
  topics,
  resources,
  practices,
  loading,
  onAddTopic,
  onAddResource,
  onAddPractice,
  onCompletePractice,
}) {
  if (skills.length === 0) return null;

  return (
    <section className="space-y-3">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">Trilha de estudo</h2>
          <p className="text-xs text-muted-foreground">Organize fontes, conceitos e etapas para cada habilidade.</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <Select value={selectedSkillId} onValueChange={onSelectSkill}>
            <SelectTrigger className="h-8 w-48 text-sm"><SelectValue /></SelectTrigger>
            <SelectContent>
              {skills.map((skill) => (
                <SelectItem key={skill.id} value={skill.id}>{skill.title}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button size="sm" variant="outline" onClick={onAddTopic}>
            <Plus className="mr-1 h-4 w-4" /> Topico
          </Button>
          <Button size="sm" variant="outline" onClick={onAddResource}>
            <Plus className="mr-1 h-4 w-4" /> Recurso
          </Button>
          <Button size="sm" onClick={onAddPractice}>
            <Plus className="mr-1 h-4 w-4" /> Pratica
          </Button>
        </div>
      </div>

      {loading ? (
        <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
          <Skeleton className="h-40 rounded-xl" />
          <Skeleton className="h-40 rounded-xl" />
          <Skeleton className="h-40 rounded-xl" />
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-semibold">Topicos</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {topics.length === 0 && <p className="py-4 text-center text-xs text-muted-foreground">Nenhum topico cadastrado.</p>}
              {topics.map((topic) => <StudyTopicRow key={topic.id} topic={topic} />)}
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-semibold">Recursos</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {resources.length === 0 && <p className="py-4 text-center text-xs text-muted-foreground">Nenhum recurso cadastrado.</p>}
              {resources.map((resource) => <StudyResourceRow key={resource.id} resource={resource} />)}
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-semibold">Pratica e Feynman</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {practices.length === 0 && <p className="py-4 text-center text-xs text-muted-foreground">Nenhuma pratica cadastrada.</p>}
              {practices.map((practice) => (
                <StudyPracticeRow key={practice.id} practice={practice} onComplete={() => onCompletePractice(practice)} />
              ))}
            </CardContent>
          </Card>
        </div>
      )}
    </section>
  );
}

function StudyTopicRow({ topic }) {
  return (
    <div className="rounded-lg border p-3">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-medium">{topic.title}</p>
          <div className="mt-1 flex flex-wrap gap-2 text-[11px] text-muted-foreground">
            <span>{STAGES[topic.stage]}</span>
            <span>{TOPIC_STATUS[topic.status]}</span>
            <span>Importancia {topic.importance}/5</span>
            <span>Confianca {topic.confidence}/5</span>
          </div>
        </div>
      </div>
      {topic.notes && <p className="mt-2 line-clamp-2 text-xs text-muted-foreground">{topic.notes}</p>}
    </div>
  );
}

function StudyResourceRow({ resource }) {
  return (
    <div className="rounded-lg border p-3">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-medium">{resource.title}</p>
          <div className="mt-1 flex flex-wrap gap-2 text-[11px] text-muted-foreground">
            <span>{RESOURCE_TYPES[resource.type]}</span>
            <span>{RESOURCE_STATUS[resource.status]}</span>
            <span>Confianca fonte {resource.reliability}/5</span>
          </div>
        </div>
        {resource.url && (
          <a className="shrink-0 text-xs font-medium text-primary" href={resource.url} target="_blank" rel="noreferrer">Abrir</a>
        )}
      </div>
      {resource.notes && <p className="mt-2 line-clamp-2 text-xs text-muted-foreground">{resource.notes}</p>}
    </div>
  );
}

function StudyPracticeRow({ practice, onComplete }) {
  const completed = practice.status === 3;

  return (
    <div className="rounded-lg border p-3">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-medium">{practice.title}</p>
          <div className="mt-1 flex flex-wrap gap-2 text-[11px] text-muted-foreground">
            <span className={cn('rounded-full px-1.5 py-0.5 font-medium', PRACTICE_STATUS_COLORS[practice.status])}>
              {PRACTICE_STATUS[practice.status]}
            </span>
            <span>Vence {new Date(practice.dueDate).toLocaleDateString('pt-BR')}</span>
            <span>Dificuldade {practice.difficulty}/5</span>
            {practice.resultScore && <span>Resultado {practice.resultScore}/5</span>}
          </div>
        </div>
        {!completed && (
          <Button size="sm" variant="outline" onClick={onComplete}>
            <CheckCircle2 className="mr-1 h-4 w-4" /> Concluir
          </Button>
        )}
      </div>
      {practice.instructions && <p className="mt-2 line-clamp-2 text-xs text-muted-foreground">{practice.instructions}</p>}
      {practice.mistakes && (
        <p className="mt-2 line-clamp-2 rounded-md bg-amber-50 p-2 text-xs text-amber-800">
          Erros: {practice.mistakes}
        </p>
      )}
    </div>
  );
}

function StudyReviewCard({ review, onComplete }) {
  return (
    <Card className="border-amber-200 bg-amber-50/40 transition-shadow hover:shadow-md dark:border-amber-900/60 dark:bg-amber-950/20">
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <CardTitle className="truncate text-base font-semibold">{review.skillTitle}</CardTitle>
            <div className="mt-1 flex flex-wrap items-center gap-1.5">
              <span className="rounded-full bg-amber-100 px-1.5 py-0.5 text-[11px] font-medium text-amber-700">
                Venceu {new Date(review.dueDate).toLocaleDateString('pt-BR')}
              </span>
              <span className="text-[11px] text-muted-foreground">Revisao #{(review.reviewCount ?? 0) + 1}</span>
            </div>
          </div>
          <Button size="sm" onClick={onComplete}>
            <CheckCircle2 className="mr-1 h-4 w-4" /> Revisar
          </Button>
        </div>
      </CardHeader>
      <CardContent className="space-y-2">
        <p className="text-sm font-medium">{review.title}</p>
        {review.prompt && <p className="line-clamp-2 text-sm text-muted-foreground">{review.prompt}</p>}
      </CardContent>
    </Card>
  );
}

function StudySessionsSection({ sessions, onFinish, onCreate, canCreate }) {
  return (
    <section className="space-y-3">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">Sessoes de estudo</h2>
          <p className="text-xs text-muted-foreground">Planeje blocos e finalize com foco, energia, Feynman e proxima acao.</p>
        </div>
        <Button size="sm" variant="outline" onClick={onCreate} disabled={!canCreate}>
          <Plus className="mr-1 h-4 w-4" /> Planejar
        </Button>
      </div>

      {sessions.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center py-10 text-center">
            <Clock className="mb-3 h-7 w-7 text-muted-foreground/50" />
            <p className="text-sm font-semibold">Nenhuma sessao planejada</p>
            <p className="mt-1 max-w-sm text-xs text-muted-foreground">
              Crie um bloco pequeno para transformar prioridade em execucao real.
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
          {sessions.map((session) => (
            <StudySessionCard key={session.id} session={session} onFinish={() => onFinish(session)} />
          ))}
        </div>
      )}
    </section>
  );
}

function StudySessionCard({ session, onFinish }) {
  const completed = session.status === 2;
  const actual = session.actualMinutes ?? 0;
  const planned = session.plannedMinutes ?? 0;
  const pct = planned > 0 ? Math.min(100, Math.round((actual / planned) * 100)) : 0;

  return (
    <Card className="transition-shadow hover:shadow-md">
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <CardTitle className="truncate text-base font-semibold">{session.skillTitle}</CardTitle>
            <div className="mt-1 flex flex-wrap items-center gap-1.5">
              <span className={cn('rounded-full px-1.5 py-0.5 text-[11px] font-medium', SESSION_STATUS_COLORS[session.status])}>
                {SESSION_STATUS[session.status]}
              </span>
              <span className="text-[11px] text-muted-foreground">{STAGES[session.stage]}</span>
              <span className="text-[11px] text-muted-foreground">
                {new Date(session.date).toLocaleDateString('pt-BR')}
              </span>
            </div>
          </div>
          {!completed && (
            <Button size="sm" onClick={onFinish}>
              <CheckCircle2 className="mr-1 h-4 w-4" /> Finalizar
            </Button>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {session.summary && <p className="line-clamp-2 text-sm text-muted-foreground">{session.summary}</p>}
        <div>
          <div className="mb-1 flex justify-between text-xs text-muted-foreground">
            <span>Tempo</span>
            <span>{completed ? `${actual}/${planned} min` : `${planned} min planejados`}</span>
          </div>
          <Progress value={completed ? pct : 0} className="h-2" indicatorClassName={completed ? 'bg-emerald-500' : 'bg-blue-500'} />
        </div>
        {completed && (
          <div className="flex flex-wrap gap-3 text-xs text-muted-foreground">
            <span>Foco {session.focusScore}/5</span>
            <span>Energia {session.energyScore}/5</span>
            <span>Dificuldade {session.difficultyScore}/5</span>
            <span>+{session.xpGenerated} XP</span>
          </div>
        )}
        {session.nextAction && (
          <div className="rounded-md bg-muted p-2 text-xs text-muted-foreground">
            Proxima acao: {session.nextAction}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function EmptyStudies({ onAdd }) {
  return (
    <div className="flex flex-col items-center py-16 text-center">
      <div className="mb-4 rounded-full bg-muted p-5">
        <BookOpen className="h-7 w-7 text-muted-foreground/40" />
      </div>
      <p className="mb-1 text-base font-semibold text-foreground">Nenhuma habilidade cadastrada</p>
      <p className="mb-4 max-w-sm text-sm text-muted-foreground">
        Comece listando o que voce quer aprender. Depois o Aurora ajuda a escolher as 3 prioridades.
      </p>
      <Button onClick={onAdd}>
        <Plus className="mr-1 h-4 w-4" /> Criar primeira habilidade
      </Button>
    </div>
  );
}

function StudySkillForm({ api, onClose, onCreated }) {
  const [form, setForm] = useState({
    title: '',
    category: '2',
    purpose: '',
    currentLevel: '',
    targetLevel: '',
    targetDate: '',
    weeklyTimeBudgetMinutes: '180',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    setError('');
    try {
      await api.post('/api/studies/skills', {
        title: form.title,
        category: parseInt(form.category),
        purpose: form.purpose || null,
        currentLevel: form.currentLevel || null,
        targetLevel: form.targetLevel || null,
        targetDate: form.targetDate || null,
        weeklyTimeBudgetMinutes: parseInt(form.weeklyTimeBudgetMinutes || '0'),
      });
      onCreated();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Nova habilidade de estudo</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="space-y-2">
            <Label>Habilidade</Label>
            <Input value={form.title} onChange={(event) => set('title', event.target.value)} placeholder="Ex: Ingles, Programacao, Matematica" />
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>Categoria</Label>
              <Select value={form.category} onValueChange={(value) => set('category', value)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(CATEGORIES).map(([value, label]) => (
                    <SelectItem key={value} value={value}>{label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Minutos por semana</Label>
              <Input type="number" min="0" value={form.weeklyTimeBudgetMinutes} onChange={(event) => set('weeklyTimeBudgetMinutes', event.target.value)} />
            </div>
          </div>
          <div className="space-y-2">
            <Label>Proposito</Label>
            <textarea
              className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={form.purpose}
              onChange={(event) => set('purpose', event.target.value)}
              placeholder="Por que isso importa agora?"
            />
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="space-y-2">
              <Label>Nivel atual</Label>
              <Input value={form.currentLevel} onChange={(event) => set('currentLevel', event.target.value)} placeholder="Basico" />
            </div>
            <div className="space-y-2">
              <Label>Nivel alvo</Label>
              <Input value={form.targetLevel} onChange={(event) => set('targetLevel', event.target.value)} placeholder="Intermediario" />
            </div>
            <div className="space-y-2">
              <Label>Prazo</Label>
              <Input type="date" value={form.targetDate} onChange={(event) => set('targetDate', event.target.value)} />
            </div>
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Salvando...' : 'Salvar'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function StudySessionForm({ api, skills, onClose, onCreated }) {
  const today = new Date().toISOString().slice(0, 10);
  const [form, setForm] = useState({
    skillId: skills[0]?.id ?? '',
    date: today,
    plannedMinutes: '45',
    stage: '4',
    summary: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    if (!form.skillId) return;
    setSaving(true);
    setError('');
    try {
      await api.post('/api/studies/sessions', {
        skillId: form.skillId,
        date: form.date,
        plannedMinutes: parseInt(form.plannedMinutes || '0'),
        stage: parseInt(form.stage),
        summary: form.summary || null,
      });
      onCreated();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Planejar sessao de estudo</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>Habilidade</Label>
              <Select value={form.skillId} onValueChange={(value) => set('skillId', value)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {skills.map((skill) => (
                    <SelectItem key={skill.id} value={skill.id}>{skill.title}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Etapa</Label>
              <Select value={form.stage} onValueChange={(value) => set('stage', value)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(STAGES).map(([value, label]) => (
                    <SelectItem key={value} value={value}>{label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>Data</Label>
              <Input type="date" value={form.date} onChange={(event) => set('date', event.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Minutos planejados</Label>
              <Input type="number" min="5" value={form.plannedMinutes} onChange={(event) => set('plannedMinutes', event.target.value)} />
            </div>
          </div>
          <div className="space-y-2">
            <Label>Objetivo do bloco</Label>
            <textarea
              className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={form.summary}
              onChange={(event) => set('summary', event.target.value)}
              placeholder="Ex: resolver 10 exercicios, revisar mapa mental, explicar um conceito..."
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Salvando...' : 'Planejar'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function StudyTopicForm({ api, skillId, onClose, onCreated }) {
  const [form, setForm] = useState({
    title: '',
    stage: '1',
    importance: '3',
    confidence: '1',
    notes: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    setError('');
    try {
      await api.post(`/api/studies/skills/${skillId}/topics`, {
        title: form.title,
        parentTopicId: null,
        stage: parseInt(form.stage),
        importance: parseInt(form.importance),
        confidence: parseInt(form.confidence),
        notes: form.notes || null,
      });
      onCreated();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Novo topico</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="space-y-2">
            <Label>Topico</Label>
            <Input value={form.title} onChange={(event) => set('title', event.target.value)} placeholder="Ex: Phrasal verbs, Clean Architecture, Porcentagem" />
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="space-y-2">
              <Label>Etapa</Label>
              <Select value={form.stage} onValueChange={(value) => set('stage', value)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(STAGES).map(([value, label]) => <SelectItem key={value} value={value}>{label}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <ScoreField label="Importancia" value={form.importance} onChange={(value) => set('importance', value)} />
            <ScoreField label="Confianca" value={form.confidence} onChange={(value) => set('confidence', value)} />
          </div>
          <div className="space-y-2">
            <Label>Notas</Label>
            <textarea className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" value={form.notes} onChange={(event) => set('notes', event.target.value)} />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Salvando...' : 'Salvar'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function StudyResourceForm({ api, skillId, onClose, onCreated }) {
  const [form, setForm] = useState({
    title: '',
    type: '1',
    url: '',
    author: '',
    reliability: '3',
    sortOrder: '0',
    notes: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    setError('');
    try {
      await api.post(`/api/studies/skills/${skillId}/resources`, {
        title: form.title,
        type: parseInt(form.type),
        url: form.url || null,
        author: form.author || null,
        reliability: parseInt(form.reliability),
        sortOrder: parseInt(form.sortOrder || '0'),
        notes: form.notes || null,
      });
      onCreated();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Novo recurso</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="space-y-2">
            <Label>Recurso</Label>
            <Input value={form.title} onChange={(event) => set('title', event.target.value)} placeholder="Ex: Curso Professor Kenny, Docs Microsoft, livro base" />
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="space-y-2">
              <Label>Tipo</Label>
              <Select value={form.type} onValueChange={(value) => set('type', value)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(RESOURCE_TYPES).map(([value, label]) => <SelectItem key={value} value={value}>{label}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <ScoreField label="Confianca fonte" value={form.reliability} onChange={(value) => set('reliability', value)} />
            <div className="space-y-2">
              <Label>Ordem</Label>
              <Input type="number" min="0" value={form.sortOrder} onChange={(event) => set('sortOrder', event.target.value)} />
            </div>
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>URL</Label>
              <Input value={form.url} onChange={(event) => set('url', event.target.value)} placeholder="https://..." />
            </div>
            <div className="space-y-2">
              <Label>Autor/Fonte</Label>
              <Input value={form.author} onChange={(event) => set('author', event.target.value)} />
            </div>
          </div>
          <div className="space-y-2">
            <Label>Notas</Label>
            <textarea className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" value={form.notes} onChange={(event) => set('notes', event.target.value)} />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Salvando...' : 'Salvar'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function StudyPracticeForm({ api, skillId, topics, onClose, onCreated }) {
  const today = new Date().toISOString().slice(0, 10);
  const [form, setForm] = useState({
    title: '',
    topicId: '_none',
    instructions: '',
    dueDate: today,
    difficulty: '3',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    setError('');
    try {
      await api.post(`/api/studies/skills/${skillId}/practice-tasks`, {
        topicId: form.topicId === '_none' ? null : form.topicId,
        title: form.title,
        instructions: form.instructions || null,
        dueDate: form.dueDate,
        difficulty: parseInt(form.difficulty),
      });
      onCreated();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Nova pratica</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="space-y-2">
            <Label>Tarefa pratica</Label>
            <Input value={form.title} onChange={(event) => set('title', event.target.value)} placeholder="Ex: explicar JWT sem consultar, resolver 10 exercicios..." />
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="space-y-2 sm:col-span-2">
              <Label>Topico vinculado</Label>
              <Select value={form.topicId} onValueChange={(value) => set('topicId', value)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="_none">Sem topico</SelectItem>
                  {topics.map((topic) => (
                    <SelectItem key={topic.id} value={topic.id}>{topic.title}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <ScoreField label="Dificuldade" value={form.difficulty} onChange={(value) => set('difficulty', value)} />
          </div>
          <div className="space-y-2">
            <Label>Prazo</Label>
            <Input type="date" value={form.dueDate} onChange={(event) => set('dueDate', event.target.value)} />
          </div>
          <div className="space-y-2">
            <Label>Instrucoes</Label>
            <textarea
              className="min-h-24 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={form.instructions}
              onChange={(event) => set('instructions', event.target.value)}
              placeholder="Defina uma entrega observavel: exercicios, mini-projeto, explicacao gravada ou resolucao sem consulta."
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Salvando...' : 'Criar pratica'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function CompletePracticeModal({ api, practice, onClose, onCompleted }) {
  const [form, setForm] = useState({
    resultScore: '3',
    submissionNotes: '',
    feynmanExplanation: '',
    mistakes: '',
    doubts: '',
    nextAction: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError('');
    try {
      await api.patch(`/api/studies/practice-tasks/${practice.id}/complete`, {
        resultScore: parseInt(form.resultScore),
        submissionNotes: form.submissionNotes || null,
        feynmanExplanation: form.feynmanExplanation || null,
        mistakes: form.mistakes || null,
        doubts: form.doubts || null,
        nextAction: form.nextAction || null,
      });
      onCompleted();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Concluir pratica</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="rounded-md bg-muted p-3">
            <p className="text-sm font-semibold">{practice.title}</p>
            {practice.instructions && <p className="mt-1 text-xs text-muted-foreground">{practice.instructions}</p>}
          </div>
          <ScoreField label="Resultado" value={form.resultScore} onChange={(value) => set('resultScore', value)} />
          <div className="space-y-2">
            <Label>Entrega realizada</Label>
            <textarea className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" value={form.submissionNotes} onChange={(event) => set('submissionNotes', event.target.value)} placeholder="O que voce fez, resolveu ou produziu?" />
          </div>
          <div className="space-y-2">
            <Label>Explicacao Feynman</Label>
            <textarea className="min-h-24 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" value={form.feynmanExplanation} onChange={(event) => set('feynmanExplanation', event.target.value)} placeholder="Explique como se estivesse ensinando para alguem iniciante." />
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="space-y-2">
              <Label>Erros percebidos</Label>
              <textarea className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" value={form.mistakes} onChange={(event) => set('mistakes', event.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Duvidas abertas</Label>
              <textarea className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" value={form.doubts} onChange={(event) => set('doubts', event.target.value)} />
            </div>
          </div>
          <div className="space-y-2">
            <Label>Proxima acao</Label>
            <Input value={form.nextAction} onChange={(event) => set('nextAction', event.target.value)} placeholder="Ex: refazer sem consulta, revisar juros compostos, criar flashcards..." />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Salvando...' : 'Concluir'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function FinishSessionModal({ api, session, onClose, onFinished }) {
  const [form, setForm] = useState({
    actualMinutes: String(session.plannedMinutes ?? 45),
    focusScore: '4',
    energyScore: '4',
    difficultyScore: '3',
    summary: session.summary ?? '',
    feynmanExplanation: '',
    nextAction: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError('');
    try {
      await api.patch(`/api/studies/sessions/${session.id}/finish`, {
        actualMinutes: parseInt(form.actualMinutes || '0'),
        focusScore: parseInt(form.focusScore),
        energyScore: parseInt(form.energyScore),
        difficultyScore: parseInt(form.difficultyScore),
        summary: form.summary || null,
        feynmanExplanation: form.feynmanExplanation || null,
        nextAction: form.nextAction || null,
      });
      onFinished();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Finalizar sessao</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="rounded-md bg-muted p-3 text-sm">
            <p className="font-medium">{session.skillTitle}</p>
            <p className="text-xs text-muted-foreground">{STAGES[session.stage]} - {session.plannedMinutes} min planejados</p>
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-4">
            <div className="space-y-2">
              <Label>Minutos</Label>
              <Input type="number" min="1" value={form.actualMinutes} onChange={(event) => set('actualMinutes', event.target.value)} />
            </div>
            <ScoreField label="Foco" value={form.focusScore} onChange={(value) => set('focusScore', value)} />
            <ScoreField label="Energia" value={form.energyScore} onChange={(value) => set('energyScore', value)} />
            <ScoreField label="Dificuldade" value={form.difficultyScore} onChange={(value) => set('difficultyScore', value)} />
          </div>
          <div className="space-y-2">
            <Label>Resumo do que aconteceu</Label>
            <textarea
              className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={form.summary}
              onChange={(event) => set('summary', event.target.value)}
              placeholder="O que voce estudou ou produziu?"
            />
          </div>
          <div className="space-y-2">
            <Label>Feynman</Label>
            <textarea
              className="min-h-24 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={form.feynmanExplanation}
              onChange={(event) => set('feynmanExplanation', event.target.value)}
              placeholder="Explique com suas palavras, como se estivesse ensinando para alguem."
            />
          </div>
          <div className="space-y-2">
            <Label>Proxima acao</Label>
            <Input value={form.nextAction} onChange={(event) => set('nextAction', event.target.value)} placeholder="Ex: revisar em D+1, resolver exercicios, gravar explicacao..." />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>
              <CheckCircle2 className="mr-1 h-4 w-4" />
              {saving ? 'Salvando...' : 'Finalizar'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function CompleteReviewModal({ api, review, onClose, onCompleted }) {
  const [form, setForm] = useState({
    result: '3',
    confidenceBefore: '2',
    confidenceAfter: '4',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError('');
    try {
      await api.patch(`/api/studies/reviews/${review.id}/complete`, {
        result: parseInt(form.result),
        confidenceBefore: parseInt(form.confidenceBefore),
        confidenceAfter: parseInt(form.confidenceAfter),
      });
      onCompleted();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Concluir revisao</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <div className="rounded-md bg-muted p-3 text-sm">
            <p className="font-medium">{review.skillTitle}</p>
            <p className="mt-1 text-xs text-muted-foreground">{review.title}</p>
            {review.prompt && <p className="mt-2 text-sm">{review.prompt}</p>}
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="space-y-2">
              <Label>Resultado</Label>
              <Select value={form.result} onValueChange={(value) => set('result', value)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(REVIEW_RESULTS).map(([value, label]) => (
                    <SelectItem key={value} value={value}>{label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <ScoreField label="Antes" value={form.confidenceBefore} onChange={(value) => set('confidenceBefore', value)} />
            <ScoreField label="Depois" value={form.confidenceAfter} onChange={(value) => set('confidenceAfter', value)} />
          </div>
          <p className="text-xs text-muted-foreground">
            O resultado define a proxima revisao: repetir volta rapido, bom/facil espaçam mais.
          </p>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>
              <CheckCircle2 className="mr-1 h-4 w-4" />
              {saving ? 'Salvando...' : 'Concluir revisao'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function PriorityAssessmentModal({ api, skill, onClose, onSaved }) {
  const [form, setForm] = useState({
    impact: '3',
    urgency: '3',
    alignment: '3',
    prerequisitePower: '3',
    motivation: '3',
    applicability: '3',
    maintenanceCost: '3',
    notes: '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (key, value) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setError('');
    try {
      await api.post(`/api/studies/skills/${skill.id}/priority-assessment`, {
        impact: parseInt(form.impact),
        urgency: parseInt(form.urgency),
        alignment: parseInt(form.alignment),
        prerequisitePower: parseInt(form.prerequisitePower),
        motivation: parseInt(form.motivation),
        applicability: parseInt(form.applicability),
        maintenanceCost: parseInt(form.maintenanceCost),
        notes: form.notes || null,
      });
      onSaved();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Avaliar prioridade</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4">
          {error && <p className="rounded-md bg-destructive/10 p-2 text-sm text-destructive">{error}</p>}
          <p className="text-sm text-muted-foreground">{skill.title}</p>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <ScoreField label="Impacto" value={form.impact} onChange={(value) => set('impact', value)} />
            <ScoreField label="Urgencia" value={form.urgency} onChange={(value) => set('urgency', value)} />
            <ScoreField label="Alinhamento" value={form.alignment} onChange={(value) => set('alignment', value)} />
            <ScoreField label="Pre-requisito" value={form.prerequisitePower} onChange={(value) => set('prerequisitePower', value)} />
            <ScoreField label="Motivacao" value={form.motivation} onChange={(value) => set('motivation', value)} />
            <ScoreField label="Aplicabilidade" value={form.applicability} onChange={(value) => set('applicability', value)} />
            <ScoreField label="Custo manutencao" value={form.maintenanceCost} onChange={(value) => set('maintenanceCost', value)} />
          </div>
          <div className="space-y-2">
            <Label>Notas</Label>
            <textarea
              className="min-h-20 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={form.notes}
              onChange={(event) => set('notes', event.target.value)}
              placeholder="O que pesa nessa decisao?"
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving}>
              <CheckCircle2 className="mr-1 h-4 w-4" />
              {saving ? 'Salvando...' : 'Salvar score'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function ScoreField({ label, value, onChange }) {
  return (
    <div className="space-y-2">
      <Label>{label}</Label>
      <Select value={value} onValueChange={onChange}>
        <SelectTrigger><SelectValue /></SelectTrigger>
        <SelectContent>
          {[1, 2, 3, 4, 5].map((score) => (
            <SelectItem key={score} value={String(score)}>{score}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}

function StudiesSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between"><Skeleton className="h-9 w-32" /><Skeleton className="h-9 w-44" /></div>
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-4">
        {[0, 1, 2, 3].map((item) => <Skeleton key={item} className="h-24 rounded-xl" />)}
      </div>
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Skeleton className="h-44 rounded-xl" />
        <Skeleton className="h-44 rounded-xl" />
      </div>
    </div>
  );
}
