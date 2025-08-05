import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"
 
// Merge Tailwind classes with clsx and twMerge
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// Format date to Brazilian format
export function formatDate(input: string | number): string {
  return new Date(input).toLocaleDateString("pt-BR")
}

// Format currency to Brazilian Real
export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(amount)
}

// Truncate text with ellipsis
export function truncate(str: string, length: number): string {
  return str.length > length ? `${str.substring(0, length)}...` : str
}

// Type guard for defined values
export function isDefined<T>(value: T | null | undefined): value is T {
  return value !== null && value !== undefined
}

// Get base URL for API calls
export function getBaseUrl() {
  if (typeof window !== "undefined") return ""
  if (process.env.VERCEL_URL) return `https://${process.env.VERCEL_URL}`
  return `http://localhost:${process.env.PORT ?? 3000}`
}

// Format error messages
export function getErrorMessage(error: unknown): string {
  if (error instanceof Error) return error.message
  if (typeof error === "string") return error
  return "Ocorreu um erro inesperado"
}

// Format file sizes
export function formatBytes(bytes: number, decimals = 2): string {
  if (bytes === 0) return "0 B"
  const k = 1024
  const sizes = ["B", "KB", "MB", "GB", "TB"]
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(decimals))} ${sizes[i]}`
}

// Debounce function for performance optimization
export function debounce<T extends (...args: any[]) => void>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout | null = null
  
  return function executedFunction(...args: Parameters<T>) {
    const later = () => {
      if (timeout) clearTimeout(timeout)
      func(...args)
    }
    
    if (timeout) clearTimeout(timeout)
    timeout = setTimeout(later, wait)
  }
}

// Generate random ID
export function generateId(prefix = ""): string {
  return `${prefix}${Math.random().toString(36).substr(2, 9)}`
}

// Check if running on client side
export function isClientSide(): boolean {
  return typeof window !== "undefined"
}

// Simple email validation
export function isValidEmail(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
}

// Get initials from name
export function getInitials(name: string): string {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .substring(0, 2)
}

// Deep clone object
export function deepClone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj))
}
// Pick properties from object
export function pick<T extends object, K extends keyof T>(
  obj: T,
  ...keys: K[]
): Pick<T, K> {
  const result = {} as Pick<T, K>
  keys.forEach((key) => {
    result[key] = obj[key]
  })
  return result
}

// Omit properties from object
export function omit<T extends object, K extends keyof T>(
  obj: T,
  ...keys: K[]
): Omit<T, K> {
  const result = { ...obj }
  keys.forEach((key) => delete result[key])
  return result
}

// Group array by key
export function groupBy<T>(
  array: T[], 
  key: keyof T | ((item: T) => string | number)
): Record<string, T[]> {
  return array.reduce((result, item) => {
    const groupKey = String(
      typeof key === 'function' ? key(item) : item[key]
    )
    if (!result[groupKey]) {
      result[groupKey] = []
    }
    result[groupKey].push(item)
    return result
  }, {} as Record<string, T[]>)
}

// Array utilities
export const arrayUtils = {
  // Remove duplicates from array
  uniq: <T>(array: T[]): T[] => [...new Set(array)],
  
  // Chunk array into smaller arrays
  chunk: <T>(array: T[], size: number): T[][] => {
    const result: T[][] = []
    for (let i = 0; i < array.length; i += size) {
      result.push(array.slice(i, i + size))
    }
    return result
  },
  
  // Sort array by key
  sortBy: <T>(
    array: T[], 
    key: keyof T | ((item: T) => any),
    order: 'asc' | 'desc' = 'asc'
  ): T[] => {
    return [...array].sort((a, b) => {
      const valueA = typeof key === 'function' ? key(a) : a[key]
      const valueB = typeof key === 'function' ? key(b) : b[key]
      
      if (valueA < valueB) return order === 'asc' ? -1 : 1
      if (valueA > valueB) return order === 'asc' ? 1 : -1
      return 0
    })
  },
  
  // Shuffle array
  shuffle: <T>(array: T[]): T[] => {
    const result = [...array]
    for (let i = result.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1))
      ;[result[i], result[j]] = [result[j], result[i]]
    }
    return result
  }
}

// Object utilities
export const objectUtils = {
  // Get nested property with dot notation
  get: (obj: any, path: string, defaultValue?: any): any => {
    const keys = path.split('.')
    let result = obj
    
    for (const key of keys) {
      result = result?.[key]
      if (result === undefined) {
        return defaultValue
      }
    }
    
    return result ?? defaultValue
  },
  
  // Set nested property with dot notation
  set: (obj: any, path: string, value: any): any => {
    const keys = path.split('.')
    let current = obj
    
    for (let i = 0; i < keys.length - 1; i++) {
      const key = keys[i]
      if (!current[key] || typeof current[key] !== 'object') {
        current[key] = {}
      }
      current = current[key]
    }
    
    current[keys[keys.length - 1]] = value
    return obj
  },
  
  // Omit properties from object
  omit: <T extends object, K extends keyof T>(
    obj: T,
    ...keys: K[]
  ): Omit<T, K> => {
    const result = { ...obj }
    keys.forEach((key) => delete result[key])
    return result
  },
  
  // Pick properties from object
  pick: <T extends object, K extends keyof T>(
    obj: T,
    ...keys: K[]
  ): Pick<T, K> => {
    const result = {} as Pick<T, K>
    keys.forEach((key) => {
      result[key] = obj[key]
    })
    return result
  }
}
