export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  code?: string
  errors?: Record<string, string[]>
}

export class ApiProblemError extends Error {
  readonly problem: ProblemDetails
  readonly status: number

  constructor(problem: ProblemDetails, status: number) {
    super(problem.title || `HTTP ${status}`)
    this.name = 'ApiProblemError'
    this.problem = problem
    this.status = status
  }
}
