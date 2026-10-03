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

// The statuses in pipeline order, for dropdowns, board columns and sorting.
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

// What we send to POST / PUT. The server sets id, createdAt and updatedAt itself.
export type JobApplicationRequest = Omit<JobApplication, 'id' | 'createdAt' | 'updatedAt'>;

// One entry from GET /api/applications/{id}/history.
export interface StatusChange {
  id: number;
  status: ApplicationStatus;
  changedAt: string;
}

// How many applications reached a stage (or a later one).
export interface FunnelStep {
  status: ApplicationStatus;
  count: number;
}

// These must match the EventType enum names in the .NET API.
export type EventType = 'OnlineTest' | 'Interview' | 'AssessmentCentre' | 'Call' | 'Other';

export const EVENT_TYPE_LABELS: Record<EventType, string> = {
  OnlineTest: 'Online test',
  Interview: 'Interview',
  AssessmentCentre: 'Assessment centre',
  Call: 'Call',
  Other: 'Other',
};

export const EVENT_TYPES = Object.keys(EVENT_TYPE_LABELS) as EventType[];

// An interview, test or call for one application (GET /api/applications/{id}/events).
export interface ApplicationEvent {
  id: number;
  title: string;
  type: EventType;
  startsAt: string; // UTC, e.g. "2026-10-10T13:00:00Z"
  notes: string | null;
}

export type ApplicationEventRequest = Omit<ApplicationEvent, 'id'>;

// An event plus its company and role, for the dashboard.
export interface UpcomingEvent {
  id: number;
  applicationId: number;
  company: string;
  role: string;
  title: string;
  type: EventType;
  startsAt: string;
}

// Shape of GET /api/dashboard/summary.
export interface DashboardSummary {
  total: number;
  statusCounts: Record<ApplicationStatus, number>;
  funnel: FunnelStep[];
  upcomingDeadlines: JobApplication[];
  upcomingEvents: UpcomingEvent[];
}
