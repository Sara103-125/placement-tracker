import { ApplicationStatus } from '../models/job-application';

/** Number of days from today until a "2026-10-06" style date (negative = in the past). */
export function daysUntil(date: string): number {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const [year, month, day] = date.split('-').map(Number);
  const target = new Date(year, month - 1, day);
  return Math.round((target.getTime() - today.getTime()) / 86_400_000);
}

export type DeadlineAlert = 'overdue' | 'soon' | null;

/**
 * Whether a deadline needs attention:
 *  - 'overdue' if it has passed and the application is still open
 *  - 'soon'    if it is today or within the next 3 days
 * Finished applications (Offer / Rejected) never get an alert.
 */
export function deadlineAlert(deadline: string | null, status: ApplicationStatus): DeadlineAlert {
  if (!deadline || status === 'Offer' || status === 'Rejected') {
    return null;
  }

  const days = daysUntil(deadline);
  if (days < 0) return 'overdue';
  if (days <= 3) return 'soon';
  return null;
}

/** "Today", "Tomorrow", "In 3 days", "2 days ago". */
export function relativeDays(date: string): string {
  const days = daysUntil(date);
  if (days === 0) return 'Today';
  if (days === 1) return 'Tomorrow';
  if (days === -1) return 'Yesterday';
  return days > 0 ? `In ${days} days` : `${-days} days ago`;
}
