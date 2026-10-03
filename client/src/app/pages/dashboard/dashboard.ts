import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardSummary, STATUS_LABELS, STATUSES } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private readonly applicationService = inject(ApplicationService);

  protected readonly statuses = STATUSES;
  protected readonly statusLabels = STATUS_LABELS;

  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly errorMessage = signal('');

  ngOnInit(): void {
    this.applicationService.getDashboardSummary().subscribe({
      next: (summary) => this.summary.set(summary),
      error: () => this.errorMessage.set('Could not load the dashboard. Is the API running?'),
    });
  }

  /** "Today", "Tomorrow" or "In 3 days" for a "2026-10-06" style date. */
  protected daysLeft(deadline: string): string {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const [year, month, day] = deadline.split('-').map(Number);
    const due = new Date(year, month - 1, day);
    const days = Math.round((due.getTime() - today.getTime()) / 86_400_000);

    if (days === 0) return 'Today';
    if (days === 1) return 'Tomorrow';
    return `In ${days} days`;
  }
}
