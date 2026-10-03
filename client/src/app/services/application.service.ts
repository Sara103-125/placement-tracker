import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ApplicationEvent,
  ApplicationEventRequest,
  ApplicationStatus,
  DashboardSummary,
  JobApplication,
  JobApplicationRequest,
  StatusChange,
} from '../models/job-application';

/**
 * Talks to the .NET API. Components call these methods instead of using HttpClient directly,
 * so all the API URLs live in one place.
 */
@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private readonly http = inject(HttpClient);

  // A relative URL: the Angular dev server proxies /api to the .NET API (see proxy.conf.json).
  private readonly baseUrl = '/api/applications';

  getAll(search: string, status: string): Observable<JobApplication[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<JobApplication[]>(this.baseUrl, { params });
  }

  getById(id: number): Observable<JobApplication> {
    return this.http.get<JobApplication>(`${this.baseUrl}/${id}`);
  }

  create(request: JobApplicationRequest): Observable<JobApplication> {
    return this.http.post<JobApplication>(this.baseUrl, request);
  }

  update(id: number, request: JobApplicationRequest): Observable<JobApplication> {
    return this.http.put<JobApplication>(`${this.baseUrl}/${id}`, request);
  }

  /** Changes only the status (used by the Kanban board). */
  updateStatus(id: number, status: ApplicationStatus): Observable<JobApplication> {
    return this.http.patch<JobApplication>(`${this.baseUrl}/${id}/status`, { status });
  }

  getHistory(id: number): Observable<StatusChange[]> {
    return this.http.get<StatusChange[]>(`${this.baseUrl}/${id}/history`);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>('/api/dashboard/summary');
  }

  getEvents(applicationId: number): Observable<ApplicationEvent[]> {
    return this.http.get<ApplicationEvent[]>(`${this.baseUrl}/${applicationId}/events`);
  }

  addEvent(applicationId: number, request: ApplicationEventRequest): Observable<ApplicationEvent> {
    return this.http.post<ApplicationEvent>(`${this.baseUrl}/${applicationId}/events`, request);
  }

  deleteEvent(applicationId: number, eventId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${applicationId}/events/${eventId}`);
  }
}
