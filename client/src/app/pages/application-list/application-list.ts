import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { JobApplication, STATUS_LABELS, STATUSES } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';
import { downloadApplicationsCsv } from '../../utils/csv';
import { deadlineAlert } from '../../utils/deadline';

type SortColumn = 'company' | 'role' | 'location' | 'status' | 'dateApplied' | 'deadline';

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
  protected readonly deadlineAlert = deadlineAlert;

  protected readonly columns: { key: SortColumn; label: string }[] = [
    { key: 'company', label: 'Company' },
    { key: 'role', label: 'Role' },
    { key: 'location', label: 'Location' },
    { key: 'status', label: 'Status' },
    { key: 'dateApplied', label: 'Date applied' },
    { key: 'deadline', label: 'Deadline' },
  ];

  protected readonly applications = signal<JobApplication[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');

  protected readonly search = signal('');
  protected readonly status = signal('');
  protected readonly hasFilters = computed(() => this.search() !== '' || this.status() !== '');

  // Sorting happens in the browser: the list is already loaded, so there is no need to ask the API again.
  protected readonly sortColumn = signal<SortColumn | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('asc');

  protected readonly sortedApplications = computed(() => {
    const column = this.sortColumn();
    const list = [...this.applications()];
    if (!column) {
      return list; // API order: most recently updated first
    }

    const direction = this.sortDirection() === 'asc' ? 1 : -1;
    return list.sort((a, b) => {
      const valueA = this.sortValue(a, column);
      const valueB = this.sortValue(b, column);
      // Empty values always go to the bottom, whichever direction we sort.
      if (valueA === null) return valueB === null ? 0 : 1;
      if (valueB === null) return -1;
      return valueA < valueB ? -direction : valueA > valueB ? direction : 0;
    });
  });

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

  /** Click a header once to sort A→Z, again for Z→A. */
  protected sortBy(column: SortColumn): void {
    if (this.sortColumn() === column) {
      this.sortDirection.update((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
  }

  protected ariaSort(column: SortColumn): 'ascending' | 'descending' | 'none' {
    if (this.sortColumn() !== column) return 'none';
    return this.sortDirection() === 'asc' ? 'ascending' : 'descending';
  }

  /** Downloads exactly what is shown: current search, filter and sort order. */
  protected exportCsv(): void {
    downloadApplicationsCsv(this.sortedApplications());
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

  private sortValue(app: JobApplication, column: SortColumn): string | number | null {
    switch (column) {
      case 'status':
        return STATUSES.indexOf(app.status); // pipeline order, not alphabetical
      case 'company':
      case 'role':
      case 'location':
        return app[column]?.toLowerCase() || null;
      default:
        return app[column]; // "2026-10-06" strings sort correctly as text
    }
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
