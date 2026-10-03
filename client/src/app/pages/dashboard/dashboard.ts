import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardSummary, STATUS_LABELS, STATUSES } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';
import { deadlineAlert, relativeDays } from '../../utils/deadline';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private readonly applicationService = inject(ApplicationService);

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly deadlineAlert = deadlineAlert;
  protected readonly relativeDays = relativeDays;

  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly errorMessage = signal('');

  /** The four headline numbers at the top. */
  protected readonly kpis = computed(() => {
    const s = this.summary();
    if (!s) return null;
    const counts = s.statusCounts;
    const reached = (status: string) => s.funnel.find((f) => f.status === status)?.count ?? 0;
    const applied = reached('Applied');
    const percentOfApplied = (n: number) => (applied === 0 ? 0 : Math.round((n / applied) * 100));
    return {
      total: s.total,
      inProgress: counts.Applied + counts.OnlineTest + counts.Interview,
      interviews: reached('Interview'),
      interviewRate: percentOfApplied(reached('Interview')),
      offers: counts.Offer,
      offerRate: percentOfApplied(counts.Offer),
    };
  });

  /** One bar per status. The longest bar fills the full width; the others are drawn relative to it. */
  protected readonly statusBars = computed(() => {
    const s = this.summary();
    if (!s) return [];
    const max = Math.max(1, ...STATUSES.map((status) => s.statusCounts[status]));
    return STATUSES.map((status) => {
      const count = s.statusCounts[status];
      return {
        status,
        count,
        widthPercent: (count / max) * 100,
        shareOfTotal: s.total === 0 ? 0 : Math.round((count / s.total) * 100),
      };
    });
  });

  /** Funnel steps, each drawn relative to the first step (everything that was applied for). */
  protected readonly funnelBars = computed(() => {
    const s = this.summary();
    if (!s || s.funnel.length === 0) return [];
    const applied = s.funnel[0].count;
    return s.funnel.map((step) => ({
      ...step,
      widthPercent: applied === 0 ? 0 : (step.count / applied) * 100,
      percentOfApplied: applied === 0 ? 0 : Math.round((step.count / applied) * 100),
    }));
  });

  ngOnInit(): void {
    this.applicationService.getDashboardSummary().subscribe({
      next: (summary) => this.summary.set(summary),
      error: () => this.errorMessage.set('Could not load the dashboard. Is the API running?'),
    });
  }
}
