"use client"

import * as React from "react"
import { Moon, Sun } from "lucide-react"
import { useTheme } from "next-themes"

import { Button } from "@/components/ui/button"

export function ThemeToggle() {
  const { theme, setTheme } = useTheme()
  const [mounted, setMounted] = React.useState(false)

  React.useEffect(() => {
    setMounted(true)
  }, [])

  const handleToggle = () => {
    const isDark = document.documentElement.classList.contains('dark');
    
    console.log('🌙 ThemeToggle clicado!');
    console.log('📊 Classe atual:', isDark ? 'dark' : 'light');
    
    if (isDark) {
      document.documentElement.classList.remove('dark');
      localStorage.setItem('theme', 'light');
      setTheme('light');
      console.log('🌞 Mudou para LIGHT');
    } else {
      document.documentElement.classList.add('dark');
      localStorage.setItem('theme', 'dark');
      setTheme('dark');
      console.log('🌙 Mudou para DARK');
    }
  }

  if (!mounted) {
    return (
      <Button variant="ghost" size="icon">
        <Sun className="h-[1.2rem] w-[1.2rem] text-gray-700 dark:text-gray-200" />
        <span className="sr-only">Toggle theme</span>
      </Button>
    )
  }

  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={handleToggle}
      className="text-gray-700 dark:text-gray-200 hover:text-gray-900 dark:hover:text-white"
    >
      <Sun className="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
      <Moon className="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
      <span className="sr-only">Toggle theme</span>
    </Button>
  )
}
