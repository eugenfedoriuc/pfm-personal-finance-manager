import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Budget, UpsertBudgetRequest } from '../models/budget';

@Injectable({ providedIn: 'root' })
export class BudgetApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/budgets';

  getForMonth(year: number, month: number): Observable<Budget[]> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<Budget[]>(this.baseUrl, { params });
  }

  upsert(request: UpsertBudgetRequest): Observable<Budget> {
    return this.http.put<Budget>(this.baseUrl, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
