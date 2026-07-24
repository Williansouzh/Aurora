import { CheckCircle2 } from 'lucide-react';
import { useState } from 'react';
import { Button } from '../../components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../../components/ui/dialog';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../components/ui/Select';
import { STAGES } from './constants';

const REVIEW_RESULTS = {
  1: 'Repetir',
  2: 'Dificil',
  3: 'Bom',
  4: 'Facil',
};

export function ScoreField({ label, value, onChange }) {
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

export function CompletePracticeModal({ api, practice, onClose, onCompleted }) {
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

export function FinishSessionModal({ api, session, onClose, onFinished }) {
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

export function CompleteReviewModal({ api, review, onClose, onCompleted }) {
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
            O resultado define a proxima revisao: repetir volta rapido, bom/facil espaçam mais.
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

export function PriorityAssessmentModal({ api, skill, onClose, onSaved }) {
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
