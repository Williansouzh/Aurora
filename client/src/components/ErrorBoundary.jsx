import React from 'react';

export class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { error: null };
  }

  static getDerivedStateFromError(error) {
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
