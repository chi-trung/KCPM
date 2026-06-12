/**
 * API Configuration
 * Uses environment variable NEXT_PUBLIC_API_URL for the base URL
 * Falls back to http://localhost:8080 for local development
 * The `/api` prefix is automatically appended
 */

// Safe environment variable access for Next.js
declare global {
    var NEXT_PUBLIC_API_URL: string | undefined;
}

// Get API URL - works on both server and client in Next.js 14
const getApiUrl = (): string => {
    return process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080";
};

export const API_CONFIG = {
    // Get base API URL from environment or use localhost for development
    BASE_URL: `${getApiUrl()}/api`,
    // Get the server URL (without /api prefix) for raw endpoints
    SERVER_URL: getApiUrl(),
};

// Freeze to prevent accidental mutations
Object.freeze(API_CONFIG);
