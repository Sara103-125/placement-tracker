import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApplicationStatus, JobApplicationRequest, STATUS_LABELS, STATUSES } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';

/**
 * One form used for both pages:
 *   /applications/new        -> add a new application
 *   /applications/5/edit     -> edit application 5
 */
@Component({
  selector: 'app-application-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './application-form.html',
  styleUrl: './application-form.css',
})
export class ApplicationForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly applicationService = inject(ApplicationService);

  protected readonly statuses = STATUSES;
  protected readonly statusLabels = STATUS_LABELS;

  // null when adding, the application's id when editing.
  protected editingId: number | null = null;

  protected readonly loading = signal(false);
  protected readonly notFound = signal(false);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal('');

  // The rules here match the rules on the API, so the user sees problems before saving.
  // Validators.pattern(/\S/) rejects text that is only spaces.
  protected readonly form = this.fb.nonNullable.group({
    company: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    role: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    location: ['', Validators.maxLength(200)],
    jobLink: ['', [Validators.pattern(/^https?:\/\/\S+$/), Validators.maxLength(500)]],
    dateApplied: [''],
    deadline: [''],
    status: ['Wishlist' as ApplicationStatus, Validators.required],
    notes: ['', Validators.maxLength(4000)],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editingId = Number(id);
      this.loadApplication(this.editingId);
    }
  }

  /** True when a field has been touched and is invalid, so we only show errors after the user interacts. */
  protected showError(field: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[field];
    return control.invalid && control.touched;
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched(); // reveal every error message at once
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    const request = this.toRequest();
    const save$ = this.editingId === null
      ? this.applicationService.create(request)
      : this.applicationService.update(this.editingId, request);

    save$.subscribe({
      next: () => this.router.navigate(['/applications']),
      error: () => {
        this.errorMessage.set('Could not save the application. Please check the details and try again.');
        this.saving.set(false);
      },
    });
  }

  private loadApplication(id: number): void {
    this.loading.set(true);
    this.applicationService.getById(id).subscribe({
      next: (app) => {
        // Empty strings for blank fields, because the inputs work with text.
        this.form.setValue({
          company: app.company,
          role: app.role,
          location: app.location ?? '',
          jobLink: app.jobLink ?? '',
          dateApplied: app.dateApplied ?? '',
          deadline: app.deadline ?? '',
          status: app.status,
          notes: app.notes ?? '',
        });
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  /** Turns the form's text values into what the API expects (blank text becomes null). */
  private toRequest(): JobApplicationRequest {
    const value = this.form.getRawValue();
    return {
      company: value.company.trim(),
      role: value.role.trim(),
      location: value.location.trim() || null,
      jobLink: value.jobLink.trim() || null,
      dateApplied: value.dateApplied || null,
      deadline: value.deadline || null,
      status: value.status,
      notes: value.notes.trim() || null,
    };
  }
}
