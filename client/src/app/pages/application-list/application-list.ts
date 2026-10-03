import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { JobApplication, STATUS_LABELS, STATUSES } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';

@Component({
  selector: 'app-application-list',
  imports: [DatePipe],
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
