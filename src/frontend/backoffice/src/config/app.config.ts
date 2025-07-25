// Configuration utility for environment variables
// Provides centralized configuration management with validation and dynamic port detection

interface ApiConfig {
  apiUrl: string
  signalRUrl?: string
  timeout: number
  isDevelopment: boolean
  debug: boolean
  isReverseProxy: boolean
  detectedProtocol: 'http' | 'https'
  detectedPort: number
}

interface AppConfig {
  api: ApiConfig
  environment: 'development' | 'production' | 'test'
}

// Auto-detect the best API URL based on current environment
function detectApiUrl(): string {
  // 1. Check explicit environment variable first
  if (process.env.REACT_APP_API_URL) {
    return process.env.REACT_APP_API_URL
  }

  // 2. Auto-detect based on current window location (for reverse proxy scenarios)
  if (typeof window !== 'undefined') {
    const currentProtocol = window.location.protocol // 'http:' or 'https:'
    const currentHostname = window.location.hostname

    // If we're running behind a reverse proxy (same domain), use current origin
    if (process.env.REACT_APP_USE_REVERSE_PROXY === 'true') {
      return `${currentProtocol}//${window.location.host}`
    }

    // 3. Auto-detect local development ports
    if (currentHostname === 'localhost' || currentHostname === '127.0.0.1') {
      // Try HTTPS first (port 7022), then HTTP (port 5183)
      if (currentProtocol === 'https:') {
        return 'https://localhost:7022'
      } else {
        return 'http://localhost:5183'
      }
    }

    // 4. For other domains, use current protocol with API subdomain
    const baseDomain = currentHostname.replace(/^(www\.|app\.|frontend\.)/i, '')
    
    // FIXED: Only use API subdomain for actual production domains, not development
    if (baseDomain !== 'localhost' && baseDomain !== '127.0.0.1' && !baseDomain.includes('localhost')) {
      return `${currentProtocol}//api.${baseDomain}`
    }
  }

  // 5. Fallback for server-side rendering, tests, or development
  return 'http://localhost:5183'
}

// Validate and provide defaults for API configuration
function getApiConfig(): ApiConfig {
  const apiUrl = detectApiUrl()
  const signalRUrl = process.env.REACT_APP_SIGNALR_URL || `${apiUrl}/flighthub`
  const environment = process.env.REACT_APP_ENV || process.env.NODE_ENV || 'development'

  // Parse URL details
  let detectedProtocol: 'http' | 'https' = 'http'
  let detectedPort = 80
  let isReverseProxy = false

  try {
    const url = new URL(apiUrl)
    detectedProtocol = url.protocol === 'https:' ? 'https' : 'http'
    detectedPort = url.port ? parseInt(url.port) : (detectedProtocol === 'https' ? 443 : 80)

    // Detect reverse proxy scenario
    if (typeof window !== 'undefined') {
      isReverseProxy = url.hostname === window.location.hostname &&
                      url.port === window.location.port
    }

    // Validate SignalR URL
    new URL(signalRUrl)
  } catch (error) {
    console.warn('⚠️ Invalid URL configuration detected, using defaults')
  }

  return {
    apiUrl,
    signalRUrl,
    timeout: 10000, // 10 seconds
    isDevelopment: environment === 'development',
    debug: process.env.REACT_APP_DEBUG === 'true' || environment === 'development',
    isReverseProxy,
    detectedProtocol,
    detectedPort
  }
}

// Get complete app configuration
export function getAppConfig(): AppConfig {
  const environment = (process.env.REACT_APP_ENV || process.env.NODE_ENV || 'development') as AppConfig['environment']

  const config: AppConfig = {
    api: getApiConfig(),
    environment
  }

  // Log configuration in development
  if (config.api.debug) {
    console.log('🔧 Backoffice App Configuration:', {
      ...config,
      api: {
        ...config.api,
        // Show useful debug info
        autoDetected: !process.env.REACT_APP_API_URL,
        currentOrigin: typeof window !== 'undefined' ? window.location.origin : 'SSR',
      }
    })
  }

  return config
}

// Export individual config sections
export const config = getAppConfig()
export const { api: apiConfig } = config

// Utility function to check if running with HTTPS
export function isHttps(url: string = apiConfig.apiUrl): boolean {
  try {
    return new URL(url).protocol === 'https:'
  } catch {
    return false
  }
}

// Utility function to get protocol prefix
export function getProtocol(url: string = apiConfig.apiUrl): 'http' | 'https' {
  return isHttps(url) ? 'https' : 'http'
}

export default config
