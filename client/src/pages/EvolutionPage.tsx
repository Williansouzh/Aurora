import { Camera, FolderOpen, Image, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../components/ui/dialog';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/Select';
import { Skeleton } from '../components/ui/Skeleton';
import { useData } from '../hooks/useData';
import { cn } from '../lib/utils';

const AREA_LABELS = {
  1:'Saúde',2:'Trabalho',3:'Estudos',4:'Dinheiro',
  5:'Relacionamentos',6:'Casa',7:'Lazer',8:'Espiritualidade',9:'Projetos',
};

export function EvolutionPage({ api }) {
  const [selectedAlbum, setSelectedAlbum] = useState(null);
  const [showAlbumForm, setShowAlbumForm] = useState(false);
  const [showPhotoForm, setShowPhotoForm] = useState(false);

  const albums = useData(() => api.get('/api/evolution/albums'), []);
  const photos = useData(
    () => selectedAlbum ? api.get(`/api/evolution/albums/${selectedAlbum.id}/photos`) : Promise.resolve([]),
    [selectedAlbum?.id],
  );

  const reloadAlbums = () => { albums.reload(); };
  const reloadPhotos = () => { photos.reload(); };

  const deleteAlbum = async (id) => {
    await api.delete(`/api/evolution/albums/${id}`);
    if (selectedAlbum?.id === id) setSelectedAlbum(null);
    reloadAlbums();
  };

  const deletePhoto = async (id) => {
    await api.delete(`/api/evolution/photos/${id}`);
    reloadPhotos();
  };

  if (albums.loading) return <EvolutionSkeleton />;

  const albumList = albums.data ?? [];
  const photoList = photos.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
            <Camera className="h-5 w-5 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">Evolução</h1>
        </div>
        <Button size="sm" onClick={() => setShowAlbumForm(true)}>
          <Plus className="h-4 w-4 mr-1" /> Novo álbum
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
        {/* Albums sidebar */}
        <div className="lg:col-span-1 space-y-2">
          <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-3">Álbuns</p>
          {albumList.length === 0 && (
            <div className="text-center py-8">
              <FolderOpen className="h-8 w-8 text-muted-foreground/30 mx-auto mb-2" />
              <p className="text-xs text-muted-foreground">Nenhum álbum</p>
            </div>
          )}
          {albumList.map((a) => (
            <button
              key={a.id}
              onClick={() => setSelectedAlbum(a)}
              className={cn(
                'w-full text-left rounded-lg border p-3 transition-colors hover:bg-accent group',
                selectedAlbum?.id === a.id && 'border-primary bg-primary/5'
              )}
            >
              <div className="flex items-start justify-between">
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{a.title}</p>
                  <p className="text-[11px] text-muted-foreground">{AREA_LABELS[a.area]}</p>
                </div>
                <button
                  className="opacity-0 group-hover:opacity-100 text-muted-foreground hover:text-destructive p-0.5"
                  onClick={(e) => { e.stopPropagation(); deleteAlbum(a.id); }}
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </button>
              </div>
            </button>
          ))}
        </div>

        {/* Photos grid */}
        <div className="lg:col-span-3">
          {!selectedAlbum ? (
            <EmptySelection />
          ) : (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-base font-semibold">{selectedAlbum.title}</h2>
                  {selectedAlbum.description && (
                    <p className="text-sm text-muted-foreground">{selectedAlbum.description}</p>
                  )}
                </div>
                <Button size="sm" onClick={() => setShowPhotoForm(true)}>
                  <Plus className="h-4 w-4 mr-1" /> Adicionar foto
                </Button>
              </div>

              {photos.loading ? (
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
                  {Array.from({length: 6}).map((_,i) => <Skeleton key={i} className="aspect-square rounded-lg" />)}
                </div>
              ) : photoList.length === 0 ? (
                <EmptyPhotos onAdd={() => setShowPhotoForm(true)} />
              ) : (
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
                  {photoList.map((p) => (
                    <PhotoCard key={p.id} photo={p} onDelete={() => deletePhoto(p.id)} />
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {showAlbumForm && (
        <AlbumFormModal api={api} onClose={() => setShowAlbumForm(false)} onCreated={reloadAlbums} />
      )}
      {showPhotoForm && selectedAlbum && (
        <PhotoFormModal
          api={api}
          album={selectedAlbum}
          onClose={() => setShowPhotoForm(false)}
          onCreated={reloadPhotos}
        />
      )}
    </div>
  );
}

function PhotoCard({ photo, onDelete }) {
  return (
    <div className="group relative aspect-square rounded-lg overflow-hidden border bg-muted">
      <img
        src={photo.imageUrl}
        alt={photo.caption ?? ''}
        className="w-full h-full object-cover"
        onError={(e) => { e.currentTarget.style.display = 'none'; }}
      />
      <div className="absolute inset-0 bg-black/0 group-hover:bg-black/40 transition-colors flex items-end p-2">
        <div className="opacity-0 group-hover:opacity-100 transition-opacity w-full">
          {photo.caption && <p className="text-white text-xs truncate">{photo.caption}</p>}
          <div className="flex items-center justify-between mt-1">
            <span className="text-white/70 text-[10px]">
              {new Date(photo.date).toLocaleDateString('pt-BR')}
            </span>
            <button
              className="text-white/70 hover:text-red-400"
              onClick={onDelete}
            >
              <Trash2 className="h-3.5 w-3.5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function EmptySelection() {
  return (
    <div className="flex flex-col items-center justify-center h-64 text-center">
      <FolderOpen className="h-10 w-10 text-muted-foreground/30 mb-3" />
      <p className="text-sm text-muted-foreground">Selecione um álbum para ver as fotos</p>
    </div>
  );
}

function EmptyPhotos({ onAdd }) {
  return (
    <div className="flex flex-col items-center py-16 text-center">
      <div className="rounded-full bg-muted p-5 mb-4"><Image className="h-7 w-7 text-muted-foreground/40" /></div>
      <p className="text-sm font-medium mb-1">Nenhuma foto neste álbum</p>
      <p className="text-xs text-muted-foreground mb-3">Adicione URLs de imagens para registrar sua evolução.</p>
      <Button size="sm" onClick={onAdd}><Plus className="h-4 w-4 mr-1" /> Adicionar foto</Button>
    </div>
  );
}

function AlbumFormModal({ api, onClose, onCreated }) {
  const [form, setForm] = useState({ title: '', area: '1', description: '' });
  const [saving, setSaving] = useState(false);
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  const submit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/api/evolution/albums', {
        title: form.title, area: parseInt(form.area),
        description: form.description || null, isPrivate: true,
      });
      onCreated(); onClose();
    } finally { setSaving(false); }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Novo álbum</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Nome</Label>
            <Input autoFocus placeholder="Ex: Evolução física 2026" value={form.title} onChange={(e) => set('title', e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Área</Label>
            <Select value={form.area} onValueChange={(v) => set('area', v)}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                {Object.entries(AREA_LABELS).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}
              </SelectContent>
            </Select>
          </div>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={saving || !form.title.trim()}>{saving ? 'Criando...' : 'Criar álbum'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function PhotoFormModal({ api, album, onClose, onCreated }) {
  const [caption, setCaption] = useState('');
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState('');
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');

  const handleFile = (e) => {
    const f = e.target.files?.[0];
    if (!f) return;
    setFile(f);
    setPreview(URL.createObjectURL(f));
  };

  const submit = async (e) => {
    e.preventDefault();
    if (!file) return;
    setUploading(true); setError('');
    try {
      const formData = new FormData();
      formData.append('file', file);
      const token = (await import('../services/authMemory')).getMemoryToken();
      const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';
      const res = await fetch(`${API_BASE_URL}/api/files/upload`, {
        method: 'POST',
        headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) },
        credentials: 'include',
        body: formData,
      });
      if (!res.ok) throw new Error('Erro ao enviar imagem.');
      const body = await res.json();
      const imageUrl = body.data?.url;

      await api.post(`/api/evolution/albums/${album.id}/photos`, {
        imageUrl, caption: caption || null, date,
      });
      onCreated(); onClose();
    } catch (err) { setError(err.message); }
    finally { setUploading(false); }
  };

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader><DialogTitle>Adicionar foto</DialogTitle></DialogHeader>
        <form onSubmit={submit} className="space-y-4 mt-2">
          <div className="space-y-1.5">
            <Label>Imagem</Label>
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp,image/gif"
              onChange={handleFile}
              className="block w-full text-sm text-muted-foreground file:mr-3 file:rounded-md file:border-0 file:bg-primary/10 file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-primary hover:file:bg-primary/20"
            />
          </div>
          {preview && (
            <img src={preview} alt="preview" className="w-full h-40 object-cover rounded-lg border" />
          )}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Legenda</Label>
              <Input placeholder="Opcional" value={caption} onChange={(e) => setCaption(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Data</Label>
              <Input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
            </div>
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="outline" onClick={onClose}>Cancelar</Button>
            <Button type="submit" disabled={uploading || !file}>
              {uploading ? 'Enviando...' : 'Adicionar foto'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function EvolutionSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex justify-between"><Skeleton className="h-9 w-32" /><Skeleton className="h-8 w-28" /></div>
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
        <div className="space-y-2">{Array.from({length:3}).map((_,i) => <Skeleton key={i} className="h-16 rounded-lg" />)}</div>
        <div className="lg:col-span-3 grid grid-cols-3 gap-3">
          {Array.from({length:6}).map((_,i) => <Skeleton key={i} className="aspect-square rounded-lg" />)}
        </div>
      </div>
    </div>
  );
}
