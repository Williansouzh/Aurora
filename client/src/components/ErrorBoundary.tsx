import React from 'react';

type ErrorBoundaryProps = { children?: React.ReactNode };
type ErrorBoundaryState = { error: Error | null };

export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  render() {
    if (!this.state.error) return this.props.children;

    return (
      <main className="runtime-error">
        <section>
          <p className="eyebrow">Aurora</p>
          <h1>Erro ao carregar o frontend</h1>
          <pre>{this.state.error.message}</pre>
          <button onClick={() => window.location.reload()}>Recarregar</button>
        </section>
      </main>
    );
  }
}
