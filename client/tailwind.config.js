/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
        popover: {
          DEFAULT: 'hsl(var(--popover))',
          foreground: 'hsl(var(--popover-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
        faint: 'hsl(var(--faint))',
        ink2: 'hsl(var(--ink2))',
        mut2: 'hsl(var(--mut2))',
        line2: 'hsl(var(--line2))',
        chipline: 'hsl(var(--chipline))',
        track: 'hsl(var(--track))',
        income: {
          DEFAULT: 'hsl(var(--income))',
          soft: 'hsl(var(--income-soft))',
        },
        expense: {
          DEFAULT: 'hsl(var(--expense))',
          soft: 'hsl(var(--expense-soft))',
        },
        pending: {
          DEFAULT: 'hsl(var(--pending))',
          soft: 'hsl(var(--pending-soft))',
        },
        transfer: 'hsl(var(--transfer))',
        // Quiet semantic palette (raw hex, mirrors CSS tokens)
        aurora: {
          income: '#6f8f6a',
          expense: '#c1796a',
          pending: '#c1976a',
          transfer: '#3d4eac',
        },
        // Account "color tags" — fixed extra palette (README)
        tag: {
          purple: '#7b5cd6',
          green: '#6f8f6a',
          indigo: '#3d4eac',
          amber: '#c1976a',
          red: '#c1796a',
        },
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 3px)',
        sm: 'calc(var(--radius) - 6px)',
      },
      fontFamily: {
        sans: ['Hanken Grotesk', 'system-ui', 'sans-serif'],
        serif: ['Newsreader', 'Georgia', 'serif'],
        display: ['Newsreader', 'Georgia', 'serif'],
      },
      boxShadow: {
        card: '0 1px 2px 0 rgb(38 36 31 / 0.04), 0 1px 3px 0 rgb(38 36 31 / 0.05)',
        'card-hover': '0 6px 20px -4px rgb(38 36 31 / 0.10)',
        fab: '0 4px 14px rgba(61, 78, 172, 0.35)',
      },
      keyframes: {
        'accordion-down': {
          from: { height: '0' },
          to: { height: 'var(--radix-accordion-content-height)' },
        },
        'accordion-up': {
          from: { height: 'var(--radix-accordion-content-height)' },
          to: { height: '0' },
        },
        'fade-in': {
          from: { opacity: '0', transform: 'translateY(4px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
      },
      animation: {
        'accordion-down': 'accordion-down 0.2s ease-out',
        'accordion-up': 'accordion-up 0.2s ease-out',
        'fade-in': 'fade-in 0.2s ease-out',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
};
