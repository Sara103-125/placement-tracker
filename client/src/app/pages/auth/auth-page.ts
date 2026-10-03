import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

/**
 * One page for both /login and /register. The route says which mode to use
 * (see `data: { mode: ... }` in app.routes.ts).
 */
@Component({
  selector: 'app-auth-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.css',
})
export class AuthPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly mode: 'login' | 'register' = inject(ActivatedRoute).snapshot.data['mode'];
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal('');

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected showError(field: 'email' | 'password'): boolean {
    const control = this.form.controls[field];
    return control.invalid && control.touched;
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set('');
    const { email, password } = this.form.getRawValue();
    const request$ = this.mode === 'login' ? this.auth.login(email, password) : this.auth.register(email, password);

    request$.subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        // The API sends { message: "..." } for wrong password (401) and email already used (409).
        this.errorMessage.set(
          error.error?.message ??
            (error.status === 0 || error.status >= 500
              ? 'Cannot reach the server. Please try again in a moment.'
              : 'Something went wrong. Please check your details.'),
        );
      },
    });
  }
}
