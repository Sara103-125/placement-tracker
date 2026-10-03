import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { JobApplication } from '../models/job-application';

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
}
