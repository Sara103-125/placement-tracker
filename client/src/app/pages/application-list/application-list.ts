import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { JobApplication, STATUS_LABELS, STATUSES } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';

@Component({
  selector: 'app-application-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './application-list.html',
  styleUrl: './application-list.css',
})
export class ApplicationList implements OnInit {
  private readonly applicationService = inject(ApplicationService);

  protected readonly statuses = STATUSES;
  protected readonly statusLabels = STATUS_LABELS;

  protected readonly applications = signal<JobApplication[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');

  protected readonly search = signal('');
  protected readonly status = signal('');
  protected readonly hasFilters = computed(() => this.search() !== '' || this.status() !== '');

  private currentRequest?: Subscription;

  ngOnInit(): void {
    this.loadApplications();
  }

  protected onSearchChange(value: string): void {
    this.search.set(value.trim());
    this.loadApplications();
  }

  protected onStatusChange(value: string): void {
    this.status.set(value);
    this.loadApplications();
  }

  protected deleteApplication(app: JobApplication): void {
    // The browser's built-in confirm box: returns true for OK, false for Cancel.
    if (!confirm(`Delete ${app.company} – ${app.role}? This cannot be undone.`)) {
      return;
    }

    this.applicationService.delete(app.id).subscribe({
      // Remove the row locally instead of reloading the whole list.
      next: () => this.applications.update((list) => list.filter((a) => a.id !== app.id)),
      error: () => alert('Could not delete the application. Please try again.'),
    });
  }

  private loadApplications(): void {
    // If the user is still typing, cancel the previous request so an old, slower
    // response can never overwrite a newer one.
    this.currentRequest?.unsubscribe();
    this.loading.set(true);

    this.currentRequest = this.applicationService.getAll(this.search(), this.status()).subscribe({
      next: (applications) => {
        this.applications.set(applications);
        this.errorMessage.set('');
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load applications. Is the API running?');
        this.loading.set(false);
      },
    });
  }
}
