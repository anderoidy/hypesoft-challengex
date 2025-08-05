import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export const theme = {
  // Color palette
  colors: {
    primary: {
      DEFAULT: 'hsl(222.2 47.4% 11.2%)',
      foreground: 'hsl(210 40% 98%)',
      light: 'hsl(222.2 47.4% 20%)',
      dark: 'hsl(222.2 47.4% 5%)',
    },
    secondary: {
      DEFAULT: 'hsl(210 40% 96.1%)',
      foreground: 'hsl(222.2 47.4% 11.2%)',
      light: 'hsl(210 40% 98%)',
      dark: 'hsl(210 40% 90%)',
    },
    accent: {
      DEFAULT: 'hsl(217.2 91.2% 59.8%)',
      foreground: 'hsl(210 40% 98%)',
      light: 'hsl(217.2 91.2% 70%)',
      dark: 'hsl(217.2 91.2% 50%)',
    },
    destructive: {
      DEFAULT: 'hsl(0 84.2% 60.2%)',
      foreground: 'hsl(210 40% 98%)',
    },
    success: {
      DEFAULT: 'hsl(142.1 76.2% 36.3%)',
      foreground: 'hsl(210 40% 98%)',
    },
    warning: {
      DEFAULT: 'hsl(38 92% 50%)',
      foreground: 'hsl(0 0% 100%)',
    },
    info: {
      DEFAULT: 'hsl(199 89% 48%)',
      foreground: 'hsl(0 0% 100%)',
    },
    muted: {
      DEFAULT: 'hsl(210 40% 96.1%)',
      foreground: 'hsl(215.4 16.3% 46.9%)',
    },
    border: 'hsl(214.3 31.8% 91.4%)',
    input: 'hsl(214.3 31.8% 91.4%)',
    ring: 'hsl(217.2 91.2% 59.8%)',
    background: 'hsl(0 0% 100%)',
    foreground: 'hsl(222.2 47.4% 11.2%)',
    card: {
      DEFAULT: 'hsl(0 0% 100%)',
      foreground: 'hsl(222.2 47.4% 11.2%)',
    },
  },
  
  // Dark mode colors
  dark: {
    colors: {
      primary: {
        DEFAULT: 'hsl(210 40% 98%)',
        foreground: 'hsl(222.2 47.4% 11.2%)',
        light: 'hsl(210 40% 90%)',
        dark: 'hsl(210 40% 85%)',
      },
      secondary: {
        DEFAULT: 'hsl(222.2 47.4% 11.2%)',
        foreground: 'hsl(210 40% 98%)',
        light: 'hsl(222.2 47.4% 15%)',
        dark: 'hsl(222.2 47.4% 5%)',
      },
      accent: {
        DEFAULT: 'hsl(217.2 91.2% 70%)',
        foreground: 'hsl(0 0% 100%)',
        light: 'hsl(217.2 91.2% 80%)',
        dark: 'hsl(217.2 91.2% 50%)',
      },
      destructive: {
        DEFAULT: 'hsl(0 84.2% 60.2%)',
        foreground: 'hsl(210 40% 98%)',
      },
      success: {
        DEFAULT: 'hsl(142.1 76.2% 36.3%)',
        foreground: 'hsl(210 40% 98%)',
      },
      warning: {
        DEFAULT: 'hsl(38 92% 50%)',
        foreground: 'hsl(0 0% 100%)',
      },
      info: {
        DEFAULT: 'hsl(199 89% 48%)',
        foreground: 'hsl(0 0% 100%)',
      },
      muted: {
        DEFAULT: 'hsl(217.2 32.6% 17.5%)',
        foreground: 'hsl(215 20.2% 65.1%)',
      },
      border: 'hsl(217.2 32.6% 17.5%)',
      input: 'hsl(217.2 32.6% 17.5%)',
      ring: 'hsl(217.2 91.2% 59.8%)',
      background: 'hsl(222.2 47.4% 5%)',
      foreground: 'hsl(210 40% 98%)',
      card: {
        DEFAULT: 'hsl(222.2 47.4% 11.2%)',
        foreground: 'hsl(210 40% 98%)',
      },
    },
  },

  // Typography
  typography: {
    fontFamily: {
      sans: ['var(--font-sans)', 'sans-serif'],
      mono: ['var(--font-mono)', 'monospace'],
    },
    fontSize: {
      xs: '0.75rem',
      sm: '0.875rem',
      base: '1rem',
      lg: '1.125rem',
      xl: '1.25rem',
      '2xl': '1.5rem',
      '3xl': '1.875rem',
      '4xl': '2.25rem',
      '5xl': '3rem',
      '6xl': '3.75rem',
    },
    fontWeight: {
      normal: '400',
      medium: '500',
      semibold: '600',
      bold: '700',
    },
    lineHeight: {
      none: '1',
      tight: '1.25',
      snug: '1.375',
      normal: '1.5',
      relaxed: '1.625',
      loose: '2',
    },
  },

  // Spacing
  spacing: {
    px: '1px',
    0: '0',
    0.5: '0.125rem',
    1: '0.25rem',
    1.5: '0.375rem',
    2: '0.5rem',
    2.5: '0.625rem',
    3: '0.75rem',
    3.5: '0.875rem',
    4: '1rem',
    5: '1.25rem',
    6: '1.5rem',
    7: '1.75rem',
    8: '2rem',
    9: '2.25rem',
    10: '2.5rem',
    11: '2.75rem',
    12: '3rem',
    14: '3.5rem',
    16: '4rem',
    20: '5rem',
    24: '6rem',
    28: '7rem',
    32: '8rem',
    36: '9rem',
    40: '10rem',
    44: '11rem',
    48: '12rem',
    52: '13rem',
    56: '14rem',
    60: '15rem',
    64: '16rem',
    72: '18rem',
    80: '20rem',
    96: '24rem',
  },

  // Border radius
  borderRadius: {
    none: '0',
    sm: '0.125rem',
    DEFAULT: '0.25rem',
    md: '0.375rem',
    lg: '0.5rem',
    xl: '0.75rem',
    '2xl': '1rem',
    '3xl': '1.5rem',
    full: '9999px',
  },

  // Box shadow
  boxShadow: {
    sm: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
    DEFAULT: '0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)',
    md: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
    lg: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
    xl: '0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)',
    '2xl': '0 25px 50px -12px rgb(0 0 0 / 0.25)',
    inner: 'inset 0 2px 4px 0 rgb(0 0 0 / 0.05)',
    none: 'none',
  },

  // Z-index
  zIndex: {
    auto: 'auto',
    0: '0',
    10: '10',
    20: '20',
    30: '30',
    40: '40',
    50: '50',
  },

  // Transitions
  transition: {
    DEFAULT: 'all 0.2s ease-in-out',
    colors: 'color, background-color, border-color, text-decoration-color, fill, stroke 0.2s ease-in-out',
    opacity: 'opacity 0.2s ease-in-out',
    shadow: 'box-shadow 0.2s ease-in-out',
    transform: 'transform 0.2s ease-in-out',
  },

  // Breakpoints
  screens: {
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1536px',
  },

  // Container
  container: {
    center: true,
    padding: '2rem',
    screens: {
      sm: '640px',
      md: '768px',
      lg: '1024px',
      xl: '1280px',
      '2xl': '1400px',
    },
  },
} as const;

export type Theme = typeof theme;

export function getThemeColor(theme: 'light' | 'dark', colorPath: string) {
  const path = colorPath.split('.');
  let current: any = theme === 'dark' ? theme.dark.colors : theme.colors;
  
  for (const key of path) {
    if (current[key] === undefined) {
      console.warn(`Color '${colorPath}' not found in theme`);
      return 'currentColor';
    }
    current = current[key];
  }
  
  return current;
}

export function getTextColor(theme: 'light' | 'dark') {
  return theme === 'dark' ? theme.dark.colors.foreground : theme.colors.foreground;
}

export function getBackgroundColor(theme: 'light' | 'dark') {
  return theme === 'dark' ? theme.dark.colors.background : theme.colors.background;
}
