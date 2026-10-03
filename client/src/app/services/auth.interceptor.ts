import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Runs on every HTTP request the app makes:
 *  1. adds "Authorization: Bearer <token>" when logged in, so the API knows who we are;
 *  2. if the API answers 401 (token expired or invalid), logs out and goes to the login page.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const token = auth.token();

  const withToken = token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;

  return next(withToken).pipe(
    catchError((error: HttpErrorResponse) => {
      const isLoginAttempt = request.url.startsWith('/api/auth/');
      if (error.status === 401 && !isLoginAttempt) {
        auth.logout();
      }
      return throwError(() => error);
    }),
  );
};
