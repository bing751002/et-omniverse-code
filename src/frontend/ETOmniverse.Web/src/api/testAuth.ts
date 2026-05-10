export interface TestAuthIdentity {
  user: string
  roles?: string[]
}

export function createTestAuthHeaders(identity: TestAuthIdentity): HeadersInit {
  const headers: Record<string, string> = {
    'X-Test-User': identity.user
  }

  if (identity.roles && identity.roles.length > 0) {
    headers['X-Test-Roles'] = identity.roles.join(',')
  }

  return headers
}
