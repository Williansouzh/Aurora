import { act, renderHook } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ToastProvider, useToast } from '../hooks/useToast';

function wrapper({ children }) {
  return <ToastProvider>{children}</ToastProvider>;
}

describe('useToast', () => {
  it('deve adicionar toast ao chamar toast.success()', () => {
    const { result } = renderHook(() => useToast(), { wrapper });

    act(() => { result.current.success('Operação concluída'); });

    expect(result.current.toasts).toHaveLength(1);
    expect(result.current.toasts[0].type).toBe('success');
    expect(result.current.toasts[0].message).toBe('Operação concluída');
  });

  it('deve remover toast após 4 segundos', () => {
    vi.useFakeTimers();
    const { result } = renderHook(() => useToast(), { wrapper });

    act(() => { result.current.success('Temporário'); });
    expect(result.current.toasts).toHaveLength(1);

    act(() => { vi.advanceTimersByTime(4000); });
    expect(result.current.toasts).toHaveLength(0);

    vi.useRealTimers();
  });

  it('deve remover toast ao chamar close manualmente', () => {
    const { result } = renderHook(() => useToast(), { wrapper });

    act(() => { result.current.success('Fechar manualmente'); });
    const id = result.current.toasts[0].id;

    act(() => { result.current.remove(id); });
    expect(result.current.toasts).toHaveLength(0);
  });

  it('deve lançar erro fora do ToastProvider', () => {
    // renderHook without wrapper = no provider
    expect(() => renderHook(() => useToast())).toThrow('useToast must be used inside ToastProvider');
  });
});
