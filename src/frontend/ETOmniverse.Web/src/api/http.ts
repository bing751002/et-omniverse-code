import { ApiProblemError, type ProblemDetails } from './problemDetails'

export interface ApiClientOptions {
  baseUrl: string
  defaultHeaders?: HeadersInit
  fetchImpl?: typeof fetch
}

export interface ApiRequestOptions extends RequestInit {
  path: string
}

export class ApiClient {
  private readonly baseUrl: string
  private readonly defaultHeaders?: HeadersInit
  private readonly fetchImpl: typeof fetch

  constructor(options: ApiClientOptions) {
    this.baseUrl = options.baseUrl.replace(/\/$/, '')
    this.defaultHeaders = options.defaultHeaders
    this.fetchImpl = options.fetchImpl ?? fetch
  }

  async request<T>(options: ApiRequestOptions): Promise<T> {
    const response = await this.fetchImpl(`${this.baseUrl}${options.path}`, {
      ...options,
      headers: mergeHeaders(this.defaultHeaders, options.headers)
    })

    if (!response.ok) {
      throw new ApiProblemError(await readProblemDetails(response), response.status)
    }

    if (response.status === 204) {
      return undefined as T
    }

    return await response.json() as T
  }
}

function mergeHeaders(left?: HeadersInit, right?: HeadersInit): Headers {
  const merged = new Headers(left)
  if (right) {
    new Headers(right).forEach((value, key) => merged.set(key, value))
  }
  return merged
}

async function readProblemDetails(response: Response): Promise<ProblemDetails> {
  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('application/problem+json') || contentType.includes('application/json')) {
    return await response.json() as ProblemDetails
  }

  return {
    status: response.status,
    title: response.statusText || `HTTP ${response.status}`
  }
}
