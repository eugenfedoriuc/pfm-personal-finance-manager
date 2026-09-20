import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { MonthlySummary } from '../models/summary';

@Injectable({ providedIn: 'root' })
export class SummaryApiService {
  private readonly http = inject(HttpClient);

  get(year: number, month: number): Observable<MonthlySummary> {
    return this.http.get<MonthlySummary>(`/api/summaries/${year}/${month}`);
  }
}
