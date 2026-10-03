import { CdkDrag, CdkDragDrop, CdkDragPlaceholder, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApplicationStatus, JobApplication, STATUS_LABELS, STATUSES } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';
import { deadlineAlert } from '../../utils/deadline';

/**
 * Kanban board: one column per status. Dragging a card into another column
 * changes that application's status (PATCH /api/applications/{id}/status).
 */
@Component({
  selector: 'app-board',
  imports: [CdkDropListGroup, CdkDropList, CdkDrag, CdkDragPlaceholder, DatePipe, RouterLink],
  templateUrl: './board.html',
  styleUrl: './board.css',
})
export class Board implements OnInit {
  private readonly applicationService = inject(ApplicationService);

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly deadlineAlert = deadlineAlert;

  protected readonly applications = signal<JobApplication[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');

  // Recalculated automatically whenever `applications` changes.
  protected readonly columns = computed(() =>
    STATUSES.map((status) => ({
      status,
      cards: this.applications().filter((app) => app.status === status),
    })),
  );

  ngOnInit(): void {
    this.applicationService.getAll('', '').subscribe({
      next: (applications) => {
        this.applications.set(applications);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load applications. Is the API running?');
        this.loading.set(false);
      },
    });
  }

  protected onDrop(event: CdkDragDrop<ApplicationStatus>): void {
    const app: JobApplication = event.item.data;
    const newStatus = event.container.data;
    if (app.status === newStatus) {
      return; // dropped back into the same column
    }

    // Move the card on screen straight away, then save. If saving fails, move it back.
    const oldStatus = app.status;
    this.setStatus(app.id, newStatus);
    this.errorMessage.set('');

    this.applicationService.updateStatus(app.id, newStatus).subscribe({
      error: () => {
        this.setStatus(app.id, oldStatus);
        this.errorMessage.set(`Could not move ${app.company}. Please try again.`);
      },
    });
  }

  private setStatus(id: number, status: ApplicationStatus): void {
    this.applications.update((list) => list.map((a) => (a.id === id ? { ...a, status } : a)));
  }
}
