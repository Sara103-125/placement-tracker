// These must match the ApplicationStatus enum names in the .NET API.
export type ApplicationStatus = 'Wishlist' | 'Applied' | 'OnlineTest' | 'Interview' | 'Offer' | 'Rejected';

// Friendly text to show on screen for each status.
export const STATUS_LABELS: Record<ApplicationStatus, string> = {
  Wishlist: 'Wishlist',
  Applied: 'Applied',
  OnlineTest: 'Online Test',
  Interview: 'Interview',
  Offer: 'Offer',
  Rejected: 'Rejected',
};

// The statuses in pipeline order, for dropdowns.
export const STATUSES = Object.keys(STATUS_LABELS) as ApplicationStatus[];

// Shape of one application as returned by GET /api/applications.
export interface JobApplication {
  id: number;
  company: string;
  role: string;
  location: string | null;
  jobLink: string | null;
  dateApplied: string | null; // "2026-10-01"
  deadline: string | null;
  status: ApplicationStatus;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}
