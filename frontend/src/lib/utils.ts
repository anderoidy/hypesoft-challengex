import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge" // ✅ Import correto

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
