import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

interface AuthResponse {
  token: string;
  email: string;
}

const TOKEN_KEY = 'pt_token';
const EMAIL_KEY = 'pt_email';

/**
 * Keeps track of who is logged in. The token from the API is saved in localStorage,
 * so you stay logged in after refreshing the page (until the token expires after 7 days).
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly token = signal<string | null>(readStorage(TOKEN_KEY));
  readonly email = signal<string | null>(readStorage(EMAIL_KEY));
  readonly isLoggedIn = computed(() => this.token() !== null);

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', { email, password }).pipe(tap((r) => this.save(r)));
  }

  register(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', { email, password }).pipe(tap((r) => this.save(r)));
  }

  logout(): void {
    this.token.set(null);
    this.email.set(null);
    writeStorage(TOKEN_KEY, null);
    writeStorage(EMAIL_KEY, null);
    this.router.navigate(['/login']);
  }

  private save(response: AuthResponse): void {
    this.token.set(response.token);
    this.email.set(response.email);
    writeStorage(TOKEN_KEY, response.token);
    writeStorage(EMAIL_KEY, response.email);
  }
}

// localStorage can throw (e.g. blocked in private browsing), so wrap it.
function readStorage(key: string): string | null {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

function writeStorage(key: string, value: string | null): void {
  try {
    if (value === null) localStorage.removeItem(key);
    else localStorage.setItem(key, value);
  } catch {
    // Not saved: the user just has to log in again next visit.
  }
}
