import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApplicationEvent, EVENT_TYPE_LABELS, EVENT_TYPES, EventType } from '../../models/job-application';
import { ApplicationService } from '../../services/application.service';

/**
 * "Interviews & events" card on the edit page: lists the events for one application
 * and has a small form to add more. Used as <app-application-events [applicationId]="5" />.
 */
@Component({
  selector: 'app-application-events',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './application-events.html',
  styleUrl: './application-events.css',
})
export class ApplicationEvents implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly applicationService = inject(ApplicationService);

  readonly applicationId = input.required<number>();

  protected readonly eventTypes = EVENT_TYPES;
  protected readonly typeLabels = EVENT_TYPE_LABELS;

  protected readonly events = signal<ApplicationEvent[]>([]);
  protected readonly adding = signal(false);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal('');

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    type: ['Interview' as EventType, Validators.required],
    startsAt: ['', Validators.required], // "2026-10-10T14:00" from <input type="datetime-local">
    notes: ['', Validators.maxLength(2000)],
  });

  ngOnInit(): void {
    this.applicationService.getEvents(this.applicationId()).subscribe({
      next: (events) => this.events.set(events),
    });
  }

  protected isPast(event: ApplicationEvent): boolean {
    return new Date(event.startsAt) < new Date();
  }

  protected showError(field: 'title' | 'startsAt'): boolean {
    const control = this.form.controls[field];
    return control.invalid && control.touched;
  }

  protected add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.errorMessage.set('');

    this.applicationService
      .addEvent(this.applicationId(), {
        title: value.title.trim(),
        type: value.type,
        // The input gives local time; toISOString() converts it to UTC for the API.
        startsAt: new Date(value.startsAt).toISOString(),
        notes: value.notes.trim() || null,
      })
      .subscribe({
        next: (created) => {
          // Insert in date order so the list stays sorted.
          this.events.update((list) => [...list, created].sort((a, b) => a.startsAt.localeCompare(b.startsAt)));
          this.form.reset({ title: '', type: 'Interview', startsAt: '', notes: '' });
          this.adding.set(false);
          this.saving.set(false);
        },
        error: () => {
          this.errorMessage.set('Could not add the event. Please try again.');
          this.saving.set(false);
        },
      });
  }

  protected remove(event: ApplicationEvent): void {
    if (!confirm(`Delete "${event.title}"?`)) {
      return;
    }

    this.applicationService.deleteEvent(this.applicationId(), event.id).subscribe({
      next: () => this.events.update((list) => list.filter((e) => e.id !== event.id)),
      error: () => alert('Could not delete the event. Please try again.'),
    });
  }
}
